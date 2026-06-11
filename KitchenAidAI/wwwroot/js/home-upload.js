(function () {
    const modal = document.getElementById('uploadFileModal');
    if (!modal) {
        return;
    }

    const form = document.getElementById('upload-file-form');
    const fileInput = document.getElementById('upload-file-input');
    const pathInput = document.getElementById('document-file-path');
    const pathList = document.getElementById('document-file-suggestions');
    const descriptionInput = document.getElementById('upload-file-description');
    const dropZone = document.getElementById('upload-dropzone');
    const progressWrap = document.getElementById('upload-progress-wrap');
    const progressBar = document.getElementById('upload-progress-bar');
    const status = document.getElementById('upload-status');
    const submitButton = document.getElementById('upload-submit-btn');
    const fromDocumentButton = document.getElementById('upload-from-document-btn');

    if (!form || !fileInput || !dropZone || !progressWrap || !progressBar || !status || !submitButton || !fromDocumentButton || !pathInput || !pathList || !descriptionInput) {
        return;
    }

    const setStatus = (message, type) => {
        status.textContent = message;
        status.className = type ? `upload-status text-${type}` : 'upload-status';
    };

    const setProgress = (value) => {
        const safeValue = Math.max(0, Math.min(100, value));
        progressBar.style.width = `${safeValue}%`;
        progressBar.setAttribute('aria-valuenow', String(safeValue));
        progressBar.textContent = `${safeValue}%`;
    };

    const resetProgress = () => {
        progressWrap.classList.add('d-none');
        setProgress(0);
    };

    const showProgress = () => {
        progressWrap.classList.remove('d-none');
        setProgress(0);
    };

    const setBusy = (busy) => {
        submitButton.disabled = busy;
        fromDocumentButton.disabled = busy;
        if (!busy) {
            fileInput.value = '';
        }
    };

    const loadAutocomplete = async (term) => {
        try {
            const response = await fetch(`/api/datoteke/autocomplete?term=${encodeURIComponent(term || '')}`);
            const payload = await response.json();
            if (!response.ok || payload?.success !== true || !Array.isArray(payload?.data)) {
                return;
            }

            pathList.innerHTML = payload.data.map((item) => `<option value="${item}"></option>`).join('');
        } catch (_error) {
        }
    };

    const uploadWithXHR = () => {
        const file = fileInput.files?.[0];
        if (!file) {
            setStatus('Odaberite datoteku s racunala ili koristite upload iz /document mape.', 'warning');
            return;
        }

        const data = new FormData();
        data.append('file', file);
        data.append('opis', descriptionInput.value || '');

        setBusy(true);
        showProgress();
        setStatus('Upload je zapoceo...', 'info');

        const xhr = new XMLHttpRequest();
        xhr.open('POST', '/api/datoteke/upload');

        xhr.upload.addEventListener('progress', (event) => {
            if (!event.lengthComputable) {
                return;
            }

            const percentage = Math.round((event.loaded / event.total) * 100);
            setProgress(percentage);
        });

        xhr.addEventListener('load', () => {
            setBusy(false);
            if (xhr.status < 200 || xhr.status >= 300) {
                setStatus('Upload nije uspio. Provjerite datoteku i pokusajte ponovno.', 'danger');
                return;
            }

            setProgress(100);
            setStatus('Upload je uspjesno zavrsen.', 'success');
        });

        xhr.addEventListener('error', () => {
            setBusy(false);
            setStatus('Doslo je do greske prilikom slanja datoteke.', 'danger');
        });

        xhr.send(data);
    };

    const uploadFromDocument = async () => {
        const relativePath = (pathInput.value || '').trim();
        if (!relativePath) {
            setStatus('Unesite putanju datoteke iz /document mape.', 'warning');
            return;
        }

        setBusy(true);
        showProgress();
        setProgress(30);
        setStatus('Priprema datoteke iz /document mape...', 'info');

        try {
            const response = await fetch('/api/datoteke/upload-from-document', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    relativePath,
                    opis: descriptionInput.value || ''
                })
            });

            const payload = await response.json();
            if (!response.ok || payload?.success !== true) {
                setStatus(payload?.alert?.message || 'Upload iz /document mape nije uspio.', 'danger');
                setBusy(false);
                return;
            }

            setProgress(100);
            setStatus('Upload iz /document mape je uspjesno zavrsen.', 'success');
            setBusy(false);
        } catch (_error) {
            setStatus('Doslo je do greske prilikom ucitavanja iz /document mape.', 'danger');
            setBusy(false);
        }
    };

    dropZone.addEventListener('click', () => fileInput.click());

    dropZone.addEventListener('dragover', (event) => {
        event.preventDefault();
        dropZone.classList.add('is-dragover');
    });

    dropZone.addEventListener('dragleave', () => {
        dropZone.classList.remove('is-dragover');
    });

    dropZone.addEventListener('drop', (event) => {
        event.preventDefault();
        dropZone.classList.remove('is-dragover');
        const dropped = event.dataTransfer?.files;
        if (dropped && dropped.length > 0) {
            fileInput.files = dropped;
            setStatus(`Odabrana datoteka: ${dropped[0].name}`, 'info');
        }
    });

    fileInput.addEventListener('change', () => {
        const selected = fileInput.files?.[0];
        if (selected) {
            setStatus(`Odabrana datoteka: ${selected.name}`, 'info');
        }
    });

    pathInput.addEventListener('input', () => {
        loadAutocomplete(pathInput.value);
    });

    form.addEventListener('submit', (event) => {
        event.preventDefault();
        uploadWithXHR();
    });

    fromDocumentButton.addEventListener('click', () => {
        uploadFromDocument();
    });

    modal.addEventListener('hidden.bs.modal', () => {
        form.reset();
        resetProgress();
        setStatus('', '');
    });

    loadAutocomplete('');
})();
