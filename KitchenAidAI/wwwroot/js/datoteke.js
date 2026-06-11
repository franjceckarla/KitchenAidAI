(function () {
    const body = document.getElementById('files-table-body');
    const alertBox = document.getElementById('files-alert');
    if (!body || !alertBox) {
        return;
    }

    const showAlert = (message, type) => {
        alertBox.className = `alert alert-${type}`;
        alertBox.textContent = message;
        alertBox.classList.remove('d-none');
    };

    body.addEventListener('click', async (event) => {
        const button = event.target.closest('.js-delete');
        if (!button) {
            return;
        }

        const row = button.closest('tr[data-id]');
        if (!row) {
            return;
        }

        const id = row.getAttribute('data-id');
        if (!id) {
            return;
        }

        if (!window.confirm('Jeste li sigurni da zelite obrisati datoteku?')) {
            return;
        }

        button.disabled = true;
        try {
            const response = await fetch(`/api/datoteke/${id}`, { method: 'DELETE' });
            const payload = await response.json();
            if (!response.ok || payload?.success !== true) {
                showAlert(payload?.alert?.message ?? 'Brisanje nije uspjelo.', 'warning');
                button.disabled = false;
                return;
            }

            showAlert('Datoteka je uspjesno obrisana.', 'success');
            row.remove();
            if (!body.querySelector('tr[data-id]')) {
                body.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Nema datoteka za prikaz.</td></tr>';
            }
        } catch (_error) {
            showAlert('Doslo je do greske pri brisanju datoteke.', 'danger');
            button.disabled = false;
        }
    });
})();
