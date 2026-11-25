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

  /* ---------- AI chat (UI only)  [data-vt-chat] ----------
     Appends the user's message and fires a `vitala:chat-send` event with
     { message, appendBotMessage, showTyping } so the team can plug in the API:

       document.addEventListener('vitala:chat-send', async (e) => {
         const stopTyping = e.detail.showTyping();
         const reply = await fetch(...);          // backend team
         stopTyping();
         e.detail.appendBotMessage(reply.text);
       });
  */
  $$('[data-vt-chat]').forEach((root) => {
    const log = $('[data-vt-chat-log]', root);
    const form = $('[data-vt-chat-form]', root);
    const input = $('input', form);

    const append = (text, who) => {
      const div = document.createElement('div');
      div.className = 'vt-msg vt-msg--' + who;
      div.textContent = text;
      log.appendChild(div);
      log.scrollTop = log.scrollHeight;
      return div;
    };
    const showTyping = () => {
      const t = document.createElement('div');
      t.className = 'vt-msg vt-msg--bot vt-msg--typing';
      t.setAttribute('aria-label', 'Assistant is typing');
      t.innerHTML = '<span></span><span></span><span></span>';
      log.appendChild(t);
      log.scrollTop = log.scrollHeight;
      return () => t.remove();
    };
    const send = (text) => {
      text = (text || '').trim();
      if (!text) return;
      append(text, 'user');
      input.value = '';
      const evt = new CustomEvent('vitala:chat-send', {
        cancelable: true,
        detail: { message: text, appendBotMessage: (t) => append(t, 'bot'), showTyping },
      });
      const handled = !document.dispatchEvent(evt) || evt.defaultPrevented;
      // Demo reply only while no backend handler calls preventDefault().
      if (!handled && root.hasAttribute('data-vt-chat-demo')) {
        const stop = showTyping();
        setTimeout(() => { stop(); append('Thanks for sharing. (Demo reply — the assistant will answer here once the backend is connected.)', 'bot'); }, 900);
      }
    };

    form.addEventListener('submit', (e) => { e.preventDefault(); send(input.value); });
    $$('[data-vt-chip]', root).forEach((chip) => chip.addEventListener('click', () => {
      input.value = chip.getAttribute('data-vt-chip');
      input.focus();
    }));
    log.scrollTop = log.scrollHeight;
  });
})();
