/* =========================================================================
   Vitala — UI behaviour only (no server calls).
   Every feature is opt-in through data-* attributes, so the file is safe to
   load on any page.
   ========================================================================= */
(function () {
  'use strict';

  const $ = (sel, root = document) => root.querySelector(sel);
  const $$ = (sel, root = document) => Array.from(root.querySelectorAll(sel));

  /* ---------- Show / hide password  [data-vt-toggle-password] ---------- */
  $$('[data-vt-toggle-password]').forEach((btn) => {
    btn.addEventListener('click', () => {
      const input = $(btn.getAttribute('data-vt-toggle-password')) ||
                    btn.closest('.vt-input-wrap')?.querySelector('input');
      if (!input) return;
      const show = input.type === 'password';
      input.type = show ? 'text' : 'password';
      btn.setAttribute('aria-pressed', String(show));
      btn.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
    });
  });

  /* ---------- Landing nav (mobile)  [data-vt-nav-toggle] ---------- */
  $$('[data-vt-nav-toggle]').forEach((btn) => {
    const menu = $(btn.getAttribute('data-vt-nav-toggle'));
    btn.addEventListener('click', () => {
      const open = menu.classList.toggle('is-open');
      btn.setAttribute('aria-expanded', String(open));
    });
  });

  /* ---------- Dashboard sidebar (mobile)  [data-vt-sidebar-open/close] ---------- */
  const sidebar = $('.vt-sidebar');
  if (sidebar) {
    const setOpen = (open) => sidebar.classList.toggle('is-open', open);
    $$('[data-vt-sidebar-open]').forEach((b) => b.addEventListener('click', () => setOpen(true)));
    $$('[data-vt-sidebar-close]').forEach((b) => b.addEventListener('click', () => setOpen(false)));
    // Topbar "indent" button: collapses the sidebar on desktop, opens it on small screens.
    $$('[data-vt-sidebar-toggle]').forEach((b) => b.addEventListener('click', () => {
      if (window.matchMedia('(max-width: 960px)').matches) { setOpen(true); return; }
      const app = sidebar.closest('.vt-app');
      if (app) b.setAttribute('aria-expanded', String(!app.classList.toggle('is-collapsed')));
    }));
    document.addEventListener('keydown', (e) => { if (e.key === 'Escape') setOpen(false); });
    document.addEventListener('click', (e) => {
      if (sidebar.classList.contains('is-open') && !sidebar.contains(e.target) && !e.target.closest('[data-vt-sidebar-open]')) setOpen(false);
    });
  }

  /* ---------- Select role  [data-vt-role-form] ---------- */
  $$('[data-vt-role-form]').forEach((form) => {
    const label = $('[data-vt-role-label]', form);
    const sync = () => {
      const checked = $('input[name="role"]:checked', form);
      $$('.vt-role-card', form).forEach((c) => c.classList.toggle('is-selected', c.contains(checked)));
      if (label && checked) label.textContent = checked.dataset.label || checked.value;
    };
    form.addEventListener('change', sync);
    sync();
  });

  /* ---------- OTP inputs  [data-vt-otp] ---------- */
  $$('[data-vt-otp]').forEach((group) => {
    const inputs = $$('.vt-otp__input', group);
    const hidden = $(group.getAttribute('data-vt-otp'));
    const update = () => { if (hidden) hidden.value = inputs.map((i) => i.value).join(''); };

    inputs.forEach((input, idx) => {
      input.addEventListener('input', () => {
        input.value = input.value.replace(/\D/g, '').slice(-1);
        if (input.value && idx < inputs.length - 1) inputs[idx + 1].focus();
        update();
      });
      input.addEventListener('keydown', (e) => {
        if (e.key === 'Backspace' && !input.value && idx > 0) inputs[idx - 1].focus();
        if (e.key === 'ArrowLeft' && idx > 0) inputs[idx - 1].focus();
        if (e.key === 'ArrowRight' && idx < inputs.length - 1) inputs[idx + 1].focus();
      });
      input.addEventListener('paste', (e) => {
        const digits = (e.clipboardData.getData('text') || '').replace(/\D/g, '').slice(0, inputs.length);
        if (!digits) return;
        e.preventDefault();
        digits.split('').forEach((d, i) => { inputs[i].value = d; });
        inputs[Math.min(digits.length, inputs.length - 1)].focus();
        update();
      });
    });

    // Block submit until every box has a digit (the hidden Code input is skipped by jQuery validation).
    // Buttons with `formnovalidate` (e.g. "Resend code") are let through.
    const form = group.closest('form');
    const error = form && $('[data-valmsg-for="' + (hidden ? hidden.name : 'Code') + '"]', form);
    if (form) {
      form.addEventListener('submit', (e) => {
        update();
        if (e.submitter && e.submitter.hasAttribute('formnovalidate')) return;
        if (hidden && hidden.value.length !== inputs.length) {
          e.preventDefault();
          if (error) error.textContent = 'Enter the ' + inputs.length + '-digit code.';
          inputs.forEach((i) => i.classList.toggle('is-invalid', !i.value));
          (inputs.find((i) => !i.value) || inputs[0]).focus();
        }
      });
      inputs.forEach((i) => i.addEventListener('input', () => {
        i.classList.remove('is-invalid');
        if (error && hidden && hidden.value.length === inputs.length) error.textContent = '';
      }));
    }
  });

  /* ---------- Countdown  [data-vt-countdown="179"] ---------- */
  $$('[data-vt-countdown]').forEach((el) => {
    const out = $('[data-vt-countdown-text]', el) || el;
    const resend = $('[data-vt-resend]', el);
    const total = parseInt(el.getAttribute('data-vt-countdown'), 10) || 180;
    let timer;
    const fmt = (s) => String(Math.floor(s / 60)).padStart(2, '0') + ':' + String(s % 60).padStart(2, '0');
    const start = () => {
      let left = total;
      out.textContent = fmt(left);
      if (resend) resend.hidden = true;
      clearInterval(timer);
      timer = setInterval(() => {
        left -= 1;
        out.textContent = fmt(Math.max(left, 0));
        if (left <= 0) { clearInterval(timer); if (resend) resend.hidden = false; }
      }, 1000);
    };
    // The resend button is a normal submit/link for the backend; we only restart the timer visually.
    if (resend) resend.addEventListener('click', start);
    start();
  });

  /* ---------- Submit a GET form when one of its fields changes  <form data-vt-autosubmit> ---------- */
  $$('form[data-vt-autosubmit]').forEach((form) => {
    form.addEventListener('change', () => form.requestSubmit ? form.requestSubmit() : form.submit());
  });

  /* ---------- Confirm before submitting  <form data-vt-confirm="Question?"> ---------- */
  $$('form[data-vt-confirm]').forEach((form) => {
    form.addEventListener('submit', (e) => {
      if (!window.confirm(form.getAttribute('data-vt-confirm'))) e.preventDefault();
    });
  });

  /* ---------- Tabs  [data-vt-tabs] (buttons with aria-controls) ---------- */
  $$('[data-vt-tabs]').forEach((list) => {
    const tabs = $$('[role="tab"]', list);
    const select = (tab) => {
      tabs.forEach((t) => {
        const on = t === tab;
        t.setAttribute('aria-selected', String(on));
        const panel = document.getElementById(t.getAttribute('aria-controls'));
        if (panel) panel.hidden = !on;
        if (on && panel) panel.dispatchEvent(new CustomEvent('vitala:tab-shown', { bubbles: true }));
      });
    };
    tabs.forEach((t) => t.addEventListener('click', () => select(t)));
  });

  /* ---------- Doctor calendar  [data-vt-calendar data-events-url] (FullCalendar, loaded on the page) ----------
     Rendered the first time its tab is shown, so FullCalendar measures a visible element. */
  $$('[data-vt-calendar]').forEach((el) => {
    let calendar;
    const colors = { Pending: '#F59E0B', Confirmed: '#16A34A', Completed: '#2563EB', NoShow: '#64748B' };
    const render = () => {
      if (calendar) { calendar.updateSize(); return; }
      if (!window.FullCalendar) { el.textContent = 'The calendar could not be loaded.'; return; }
      calendar = new window.FullCalendar.Calendar(el, {
        initialView: 'timeGridWeek',
        firstDay: 6,
        slotMinTime: '07:00:00',
        slotMaxTime: '23:00:00',
        allDaySlot: false,
        height: 'auto',
        headerToolbar: { left: 'prev,next today', center: 'title', right: 'timeGridWeek,timeGridDay,dayGridMonth' },
        events: el.getAttribute('data-events-url'),
        eventDataTransform: (ev) => {
          const c = colors[ev.status] || '#14248A';
          return Object.assign({}, ev, { backgroundColor: c, borderColor: c, title: ev.title + ' · ' + ev.status });
        },
      });
      calendar.render();
    };
    const panel = el.closest('[role="tabpanel"]');
    if (panel && panel.hidden) panel.addEventListener('vitala:tab-shown', render);
    else window.addEventListener('load', render);
  });

  /* ---------- Booking time picker  [data-vt-booking data-times-url data-doctor-id] ----------
     Changing the date loads GET {times-url}?doctorId=&date=yyyy-MM-dd (array like "10:00:00")
     and redraws the time radios. Without JS the date form submits normally. */
  $$('[data-vt-booking]').forEach((root) => {
    const url = root.getAttribute('data-times-url');
    const doctorId = root.getAttribute('data-doctor-id');
    const dateInput = $('[data-vt-booking-date]', root);
    const hiddenDate = $('[data-vt-booking-date-hidden]', root);
    const slots = $('[data-vt-slots]', root);
    const empty = $('[data-vt-slots-empty]', root);
    const submit = $('[data-vt-booking-submit]', root);
    if (!dateInput || !slots) return;

    const draw = (times) => {
      slots.innerHTML = '';
      times.forEach((t, i) => {
        const hhmm = String(t).slice(0, 5);
        const label = document.createElement('label');
        label.className = 'vt-slot';
        const input = document.createElement('input');
        input.type = 'radio'; input.name = 'time'; input.value = hhmm; input.required = true; input.checked = i === 0;
        const span = document.createElement('span');
        span.textContent = hhmm;
        label.append(input, span);
        slots.appendChild(label);
      });
      if (empty) empty.hidden = times.length > 0;
      if (submit) submit.disabled = times.length === 0;
    };

    dateInput.addEventListener('change', async () => {
      const date = dateInput.value;
      if (!date) return;
      if (hiddenDate) hiddenDate.value = date;
      root.classList.add('vt-loading');
      try {
        const res = await fetch(url + '?doctorId=' + encodeURIComponent(doctorId) + '&date=' + encodeURIComponent(date), { headers: { Accept: 'application/json' } });
        if (!res.ok) throw new Error(res.status);
        draw(await res.json());
        const u = new URL(window.location.href);
        u.searchParams.set('date', date);
        window.history.replaceState(null, '', u);
      } catch (err) {
        dateInput.form.submit(); // fall back to the server-rendered page
      } finally {
        root.classList.remove('vt-loading');
      }
    });
  });

  /* ---------- Live notifications (SignalR)  <div data-vt-live-notifications data-hub-url="/hubs/notifications">
     The server pushes "notification" { id, title, message, link, unreadCount } to the signed-in user.
     Updates the sidebar badge [data-vt-unread] / topbar dot [data-vt-unread-dot] and shows a toast. ---------- */
  /* ---------- Dark / light mode  [data-vt-theme-toggle] ----------
     The <head> script applies the saved theme before paint; this keeps the buttons in sync and saves changes. */
  const root = document.documentElement;
  const syncTheme = () => {
    const dark = root.getAttribute('data-theme') === 'dark';
    $$('[data-vt-theme-toggle]').forEach((btn) => {
      btn.setAttribute('aria-pressed', String(dark));
      const label = $('[data-vt-theme-label]', btn);
      if (label) label.textContent = dark ? 'Light mode' : 'Dark mode';
      if (!label) btn.setAttribute('aria-label', dark ? 'Switch to light mode' : 'Switch to dark mode');
    });
  };
  $$('[data-vt-theme-toggle]').forEach((btn) => btn.addEventListener('click', () => {
    const dark = root.getAttribute('data-theme') !== 'dark';
    if (dark) root.setAttribute('data-theme', 'dark'); else root.removeAttribute('data-theme');
    try { localStorage.setItem('vt-theme', dark ? 'dark' : 'light'); } catch (e) { /* private mode */ }
    syncTheme();
    document.dispatchEvent(new CustomEvent('vt-theme-change', { detail: { dark } })); // e.g. charts redraw with the new colors
  }));
  syncTheme();

  // The SignalR library is loaded after this file (page Scripts section), so start once the page has loaded.
  const startLive = () => {
  const live = $('[data-vt-live-notifications]');
  if (live && window.signalR) {
    const setUnread = (count) => {
      $$('[data-vt-unread]').forEach((b) => {
        b.textContent = count > 99 ? '99+' : String(count);
        b.setAttribute('aria-label', `${count} unread`);
        b.hidden = count === 0;
      });
      $$('[data-vt-unread-dot]').forEach((d) => { d.hidden = count === 0; });
    };

    const region = document.createElement('div');
    region.className = 'vt-toasts';
    region.setAttribute('aria-live', 'polite');
    region.setAttribute('role', 'status');
    document.body.appendChild(region);

    const showToast = (n) => {
      const toast = document.createElement('div');
      toast.className = 'vt-toast';
      const title = document.createElement('strong');
      title.className = 'vt-toast__title';
      title.textContent = n.title;            // textContent: never inject server text as HTML
      const msg = document.createElement('p');
      msg.className = 'vt-toast__msg';
      msg.textContent = n.message;
      toast.append(title, msg);

      const actions = document.createElement('div');
      actions.className = 'vt-toast__actions';
      const view = document.createElement('a');
      view.className = 'vt-toast__link';
      view.href = live.dataset.listUrl || '/Notifications';
      view.textContent = 'View';
      const close = document.createElement('button');
      close.type = 'button';
      close.className = 'vt-toast__close';
      close.setAttribute('aria-label', 'Dismiss');
      close.textContent = '×';
      close.addEventListener('click', () => toast.remove());
      actions.append(view, close);
      toast.appendChild(actions);

      region.appendChild(toast);
      setTimeout(() => toast.classList.add('is-leaving'), 7500);
      setTimeout(() => toast.remove(), 8000);
    };

    const connection = new window.signalR.HubConnectionBuilder()
      .withUrl(live.dataset.hubUrl)
      .withAutomaticReconnect()
      .build();

    connection.on('notification', (n) => {
      setUnread(n.unreadCount);
      showToast(n);
    });

    connection.start().catch(() => { /* offline or signed out: the page still works without live updates */ });
  }
  };
  if (document.readyState === 'complete') startLive();
  else window.addEventListener('load', startLive, { once: true });
})();
