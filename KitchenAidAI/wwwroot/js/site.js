// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

const searchDebounce = new Map();
let dateLocaleConfig;

function wireAjaxSearch() {
	const inputs = document.querySelectorAll("[data-search-input]");
	inputs.forEach((input) => {
		input.addEventListener("input", () => {
			const key = input.dataset.searchTarget || "default";
			clearTimeout(searchDebounce.get(key));
			searchDebounce.set(key, setTimeout(() => runSearch(input), 250));
		});
	});
}

function runSearch(input) {
	const targetSelector = input.dataset.searchTarget;
	const urlValue = input.dataset.searchUrl;
	if (!targetSelector || !urlValue) {
		return;
	}

	const target = document.querySelector(targetSelector);
	if (!target) {
		return;
	}

	const url = new URL(urlValue, window.location.origin);
	if (input.value) {
		url.searchParams.set("search", input.value);
	}

	fetch(url.toString(), {
		headers: {
			"X-Requested-With": "XMLHttpRequest"
		}
	})
		.then((response) => response.text())
		.then((html) => {
			target.innerHTML = html;
		})
		.catch(() => {
			// No-op: keep the current list if the request fails.
		});
}

document.addEventListener("DOMContentLoaded", () => {
	wireAjaxSearch();
	initClientValidation();
	initJqueryDateTimePicker();
	initAutocompleteDropdowns();
	initRowLinks();
	initChefGreeting();
});

function initClientValidation() {
	if (!window.jQuery || !jQuery.validator) {
		return;
	}

	if (!dateLocaleConfig) {
		const locale = (document.documentElement.lang || navigator.language || "en-US").toLowerCase();
		dateLocaleConfig = resolveDateLocaleConfig(locale);
	}

	if (jQuery.validator.methods.date) {
		jQuery.validator.methods.date = function (value, element) {
			if (this.optional(element)) {
				return true;
			}

			const trimmed = String(value || "").trim();
			if (!trimmed) {
				return true;
			}

			const localMatch = trimmed.match(/^(\d{1,2})[./](\d{1,2})[./](\d{4})(?:\s+(\d{2}):(\d{2}))?$/);
			if (localMatch) {
				const order = dateLocaleConfig?.order || "dmy";
				const first = Number(localMatch[1]);
				const second = Number(localMatch[2]);
				const year = Number(localMatch[3]);
				const hours = Number(localMatch[4] || 0);
				const minutes = Number(localMatch[5] || 0);
				const day = order === "mdy" ? second : first;
				const month = order === "mdy" ? first : second;
				const parsed = new Date(year, month - 1, day, hours, minutes);
				return parsed.getFullYear() === year
					&& parsed.getMonth() === month - 1
					&& parsed.getDate() === day;
			}

			const isoMatch = trimmed.match(/^(\d{4})-(\d{2})-(\d{2})(?:[T\s](\d{2}):(\d{2}))?$/);
			if (isoMatch) {
				const year = Number(isoMatch[1]);
				const month = Number(isoMatch[2]);
				const day = Number(isoMatch[3]);
				const hours = Number(isoMatch[4] || 0);
				const minutes = Number(isoMatch[5] || 0);
				const parsed = new Date(year, month - 1, day, hours, minutes);
				return parsed.getFullYear() === year
					&& parsed.getMonth() === month - 1
					&& parsed.getDate() === day;
			}

			const parsed = new Date(trimmed);
			return !Number.isNaN(parsed.getTime());
		};
	}

	jQuery.validator.setDefaults({
		onfocusout(element) {
			this.element(element);
		}
	});

	if (jQuery.validator.unobtrusive) {
		jQuery.validator.unobtrusive.parse(document);
	}
}

function initJqueryDateTimePicker() {
	if (!window.jQuery || !jQuery.fn || !jQuery.fn.datetimepicker) {
		return;
	}

	const locale = (document.documentElement.lang || navigator.language || "en-US").toLowerCase();
	dateLocaleConfig = resolveDateLocaleConfig(locale);
	const formatDateTime = buildDateFormat(dateLocaleConfig, true);
	const formatDate = buildDateFormat(dateLocaleConfig, false);

	$(".datetimepicker-input").each(function () {
		const $input = $(this);
		const includeTime = $input.data("include-time") === true || $input.data("include-time") === "true";
		const prefill = $input.data("value");

		$input.datetimepicker({
			format: includeTime ? formatDateTime : formatDate,
			step: 30,
			timepicker: includeTime,
			todayButton: true,
			lang: dateLocaleConfig.lang
		});

		if (prefill) {
			const parsed = new Date(prefill);
			if (!Number.isNaN(parsed.getTime())) {
				const displayValue = formatForPicker(parsed, includeTime, dateLocaleConfig);
				$input.val(displayValue);
			}
		}
	});
}

function resolveDateLocaleConfig(locale) {
	const normalized = locale.toLowerCase();
	const configs = [
		{ match: ["hr"], order: "dmy", separator: ".", lang: "hr" },
		{ match: ["de"], order: "dmy", separator: ".", lang: "de" },
		{ match: ["fr"], order: "dmy", separator: "/", lang: "fr" },
		{ match: ["es"], order: "dmy", separator: "/", lang: "es" },
		{ match: ["en-gb", "en-ie", "en-au", "en-nz"], order: "dmy", separator: "/", lang: "en" },
		{ match: ["en-us", "en"], order: "mdy", separator: "/", lang: "en" }
	];

	for (const config of configs) {
		if (config.match.some((prefix) => normalized === prefix || normalized.startsWith(prefix))) {
			return config;
		}
	}

	return { order: "dmy", separator: ".", lang: "en" };
}

function buildDateFormat(config, includeTime) {
	const datePart = config.order === "mdy"
		? `m${config.separator}d${config.separator}Y`
		: `d${config.separator}m${config.separator}Y`;
	return includeTime ? `${datePart} H:i` : datePart;
}

function formatForPicker(date, includeTime, config) {
	const day = pad(date.getDate());
	const month = pad(date.getMonth() + 1);
	const year = date.getFullYear();
	const sep = config.separator;
	const datePart = config.order === "mdy"
		? `${month}${sep}${day}${sep}${year}`
		: `${day}${sep}${month}${sep}${year}`;

	if (!includeTime) {
		return datePart;
	}
	const hours = pad(date.getHours());
	const minutes = pad(date.getMinutes());
	return `${datePart} ${hours}:${minutes}`;
}

function pad(value) {
	return String(value).padStart(2, "0");
}

function initAutocompleteDropdowns() {
	const containers = document.querySelectorAll(".autocomplete");
	containers.forEach((container) => {
		const input = container.querySelector(".autocomplete-input");
		const menu = container.querySelector(".autocomplete-menu");
		const url = container.dataset.autocompleteUrl;
		const field = container.dataset.autocompleteField;
		const minLength = Number(container.dataset.autocompleteMinLength || 2);
		if (!input || !menu || !url) {
			return;
		}

		let activeIndex = -1;
		let pending;

		const closeMenu = () => {
			menu.classList.remove("is-open");
			menu.innerHTML = "";
			activeIndex = -1;
		};

		const openMenu = () => {
			if (menu.children.length > 0) {
				menu.classList.add("is-open");
			}
		};

		const setActive = (index) => {
			const items = menu.querySelectorAll(".autocomplete-item");
			items.forEach((item, idx) => {
				item.classList.toggle("is-active", idx === index);
			});
			activeIndex = index;
		};

		const renderItems = (items) => {
			menu.innerHTML = "";
			items.forEach((label, index) => {
				const button = document.createElement("button");
				button.type = "button";
				button.className = "autocomplete-item";
				button.textContent = label;
				button.addEventListener("click", () => {
					input.value = label;
					closeMenu();
				});
				button.addEventListener("mouseenter", () => setActive(index));
				menu.appendChild(button);
			});
			openMenu();
		};

		const fetchResults = (query) => {
			if (pending) {
				clearTimeout(pending);
			}
			pending = setTimeout(() => {
				const requestUrl = new URL(url, window.location.origin);
				requestUrl.searchParams.set("term", query);
				if (field) {
					requestUrl.searchParams.set("field", field);
				}
				fetch(requestUrl.toString())
					.then((response) => response.json())
					.then((data) => {
						if (!Array.isArray(data)) {
							closeMenu();
							return;
						}
						renderItems(data);
					})
					.catch(() => closeMenu());
			}, 200);
		};

		input.addEventListener("input", () => {
			const value = input.value.trim();
			if (value.length < minLength) {
				closeMenu();
				return;
			}
			fetchResults(value);
		});

		input.addEventListener("focus", () => {
			const value = input.value.trim();
			if (value.length >= minLength) {
				fetchResults(value);
			}
		});

		input.addEventListener("keydown", (event) => {
			const items = menu.querySelectorAll(".autocomplete-item");
			if (!items.length || !menu.classList.contains("is-open")) {
				return;
			}

			if (event.key === "ArrowDown") {
				event.preventDefault();
				const next = activeIndex + 1 >= items.length ? 0 : activeIndex + 1;
				setActive(next);
			}

			if (event.key === "ArrowUp") {
				event.preventDefault();
				const next = activeIndex - 1 < 0 ? items.length - 1 : activeIndex - 1;
				setActive(next);
			}

			if (event.key === "Enter" && activeIndex >= 0) {
				event.preventDefault();
				items[activeIndex].click();
			}

			if (event.key === "Escape") {
				closeMenu();
			}
		});

		input.addEventListener("blur", () => {
			setTimeout(closeMenu, 150);
		});
	});
}

function initRowLinks() {
	document.addEventListener("click", (event) => {
		const target = event.target;
		if (!(target instanceof Element)) {
			return;
		}

		if (target.closest("a, button, input, select, textarea, label")) {
			return;
		}

		const row = target.closest("[data-row-href]");
		if (!row) {
			return;
		}

		const href = row.getAttribute("data-row-href");
		if (href) {
			window.location.assign(href);
		}
	});
}

let chefHideTimeout;

function initChefGreeting() {
	const wrapper = document.getElementById("chef-greeting-wrapper");
	if (!wrapper) {
		return;
	}

	const closeBtn = wrapper.querySelector("#chef-close-btn");
	if (closeBtn) {
		closeBtn.addEventListener("click", hideChefGreeting);
	}

	const username = wrapper.dataset.chefUsername || "Korisnice";
	const autostart = wrapper.dataset.chefAutostart === "true";
	const autohideMs = Number(wrapper.dataset.chefAutohide || 6000);
	const context = wrapper.dataset.chefContext || "sekcija";
	const isAdmin = wrapper.dataset.chefIsAdmin === "true";

	setChefContextMessage(context, isAdmin);

	if (autostart) {
		showChefGreeting(username, autohideMs);
	}
}

function setChefContextMessage(context, isAdmin) {
	const messageEl = document.getElementById("chef-context-message");
	if (!messageEl) {
		return;
	}

	if (isAdmin) {
		messageEl.textContent = "Pregledavate pocetnu nadzornu plocu.";
		return;
	}

	const messageByContext = {
		home: "Dobrodosli nazad. Pogledajte sto je novo danas.",
		recepti: "Sada gledate recepte koji postoje u aplikaciji.",
		frizider: "Provjerite stanje vaseg frizidera i dostupne namirnice.",
		namirnice: "Upravljate namirnicama koje su trenutno unesene.",
		kuharice: "Ovdje su vase kuharice i spremljeni izbori.",
		chat: "Razgovarate s asistentom za kuhinjske ideje.",
		info: "Informacije o aplikaciji su ovdje na dohvat ruke.",
		korisnici: "Pregledavate korisnike i njihove podatke.",
		sekcija: "Nastavite s pregledom ove sekcije."
	};

	messageEl.textContent = messageByContext[context] || messageByContext.sekcija;
}

function showChefGreeting(username, autohideMs) {
	const wrapper = document.getElementById("chef-greeting-wrapper");
	if (!wrapper) {
		return;
	}

	const nameEl = wrapper.querySelector("#chef-username");
	if (nameEl && username) {
		nameEl.textContent = username;
	}

	wrapper.style.display = "flex";
	wrapper.classList.remove("hide");

	const clone = wrapper.cloneNode(true);
	wrapper.parentNode.replaceChild(clone, wrapper);

	const closeBtn = clone.querySelector("#chef-close-btn");
	if (closeBtn) {
		closeBtn.addEventListener("click", hideChefGreeting);
	}

	if (chefHideTimeout) {
		clearTimeout(chefHideTimeout);
	}

	const hideAfter = autohideMs === undefined ? 6000 : autohideMs;
	if (hideAfter > 0) {
		chefHideTimeout = setTimeout(hideChefGreeting, hideAfter);
	}
}

function hideChefGreeting() {
	const wrapper = document.getElementById("chef-greeting-wrapper");
	if (!wrapper) {
		return;
	}

	if (chefHideTimeout) {
		clearTimeout(chefHideTimeout);
	}

	wrapper.classList.add("hide");
	setTimeout(() => {
		wrapper.style.display = "none";
	}, 700);
}
