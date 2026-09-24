'use strict';
(() => {
  const form = document.getElementById('auth-form');
  const firstName = document.getElementById('first-name');
  const lastName = document.getElementById('last-name');
  const email = document.getElementById('email');
  const password = document.getElementById('password');
  const toggle = document.getElementById('toggle-password');
  const submit = document.getElementById('submit-auth');
  const resend = document.getElementById('resend-email');
  const google = document.getElementById('google-login');
  const switchMode = document.getElementById('switch-mode');
  const feedback = document.getElementById('feedback');
  const controls = document.getElementById('auth-controls');
  const success = document.getElementById('success-panel');
  const title = document.getElementById('auth-title');
  const subtitle = document.getElementById('auth-subtitle');
  const tokenKey = 'cv.accessToken';
  let busy = false;
  let signingUp = false;

  function message(text, kind = 'error') {
    feedback.textContent = text;
    feedback.className = 'feedback ' + kind;
    feedback.hidden = !text;
  }

  function resetPassword() {
    password.value = '';
    password.type = 'password';
    toggle.textContent = 'Ko‘rsatish';
    toggle.setAttribute('aria-pressed', 'false');
    toggle.setAttribute('aria-label', 'Parolni ko‘rsatish');
  }

  function setMode(signup) {
    signingUp = signup;
    const label = signup ? 'Ro‘yxatdan o‘tish' : 'Kirish';
    title.textContent = label;
    document.title = label + ' — CV Workspace';
    subtitle.textContent = signup ? 'Yangi hisob yarating.' : 'Hisobingizga kiring.';
    submit.textContent = label;
    document.getElementById('name-fields').hidden = !signup;
    for (const input of [firstName, lastName]) {
      input.disabled = !signup;
      input.required = signup;
      input.setCustomValidity('');
    }
    email.autocomplete = signup ? 'email' : 'username';
    password.autocomplete = signup ? 'new-password' : 'current-password';
    password.minLength = signup ? 8 : 1;
    if (signup) password.setAttribute('aria-describedby', 'password-hint');
    else password.removeAttribute('aria-describedby');
    document.getElementById('password-hint').hidden = !signup;
    document.getElementById('mode-prompt').textContent = signup ? 'Hisobingiz bormi?' : 'Hisobingiz yo‘qmi?';
    switchMode.textContent = signup ? 'Kirish' : 'Ro‘yxatdan o‘tish';
    switchMode.href = signup ? '/login.html' : '/login.html?mode=signup';
    resend.hidden = signup;
    resetPassword();
    message('');
  }

  function loading(value, label) {
    busy = value;
    submit.disabled = value;
    resend.disabled = value;
    for (const input of [firstName, lastName, email, password]) input.readOnly = value;
    for (const link of [google, switchMode]) link.setAttribute('aria-disabled', String(value));
    form.setAttribute('aria-busy', String(value));
    submit.textContent = value ? (label || (signingUp ? 'Hisob yaratilmoqda…' : 'Kirilmoqda…')) : (signingUp ? 'Ro‘yxatdan o‘tish' : 'Kirish');
  }

  async function request(url, data) {
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), 60000);
    try {
      const response = await fetch(url, {
        method: data === undefined ? 'GET' : 'POST',
        headers: data === undefined ? { Accept: 'application/json' } : { Accept: 'application/json', 'Content-Type': 'application/json' },
        body: data === undefined ? undefined : JSON.stringify(data),
        credentials: 'same-origin', cache: 'no-store', signal: controller.signal
      });
      let body = null;
      try { body = await response.json(); } catch { /* Non-JSON failures use the status message. */ }
      return { status: response.status, ok: response.ok, body };
    } finally { clearTimeout(timer); }
  }

  function complete(body) {
    if (!body || typeof body.accessToken !== 'string' || !body.accessToken) {
      message('Kirish javobi olinmadi. Qayta urinib ko‘ring.');
      return;
    }
    try { sessionStorage.setItem(tokenKey, body.accessToken); }
    catch { message('Brauzer sessiyani saqlashga ruxsat bermadi. Brauzer sozlamalarini tekshiring.'); return; }
    resetPassword();
    message('');
    controls.hidden = true;
    success.hidden = false;
    title.textContent = 'Hisobingizga kirdingiz';
    subtitle.textContent = 'Kirish muvaffaqiyatli yakunlandi.';
    success.focus();
  }

  function failure(result, isGoogle = false) {
    if (result.status === 403) message('Emailingiz hali tasdiqlanmagan. Xatingizdagi havolani oching yoki tasdiqlash xatini qayta yuboring.');
    else if (result.status === 401) message(isGoogle ? 'Google orqali kirish yakunlanmadi. Qayta urinib ko‘ring.' : 'Email yoki parol noto‘g‘ri. Tekshirib, qayta kiriting.');
    else if (result.status === 409) message('Bu email bilan hisob mavjud. Kirish formasidan foydalaning.');
    else if (result.status === 429) message('So‘rovlar juda ko‘p. Birozdan keyin qayta urinib ko‘ring.');
    else if (result.status === 400) message('Kiritilgan ma’lumotlarni tekshiring va qayta urinib ko‘ring.');
    else message('Server bilan bog‘lanishda muammo yuz berdi. Birozdan keyin qayta urinib ko‘ring.');
  }

  function networkError(error) {
    message(error.name === 'AbortError' ? 'So‘rov vaqti tugadi. Qayta urinib ko‘ring.' : 'Ulanishni tekshiring va qayta urinib ko‘ring.');
  }

  for (const input of [firstName, lastName]) {
    input.addEventListener('input', () => input.setCustomValidity(''));
  }
  toggle.addEventListener('click', () => {
    const visible = password.type === 'password';
    password.type = visible ? 'text' : 'password';
    toggle.textContent = visible ? 'Yashirish' : 'Ko‘rsatish';
    toggle.setAttribute('aria-pressed', String(visible));
    toggle.setAttribute('aria-label', visible ? 'Parolni yashirish' : 'Parolni ko‘rsatish');
  });
  switchMode.addEventListener('click', event => {
    event.preventDefault();
    if (busy) return;
    const target = switchMode.getAttribute('href');
    setMode(!signingUp);
    history.replaceState(null, '', target);
    (signingUp ? firstName : email).focus();
  });
  form.addEventListener('submit', async event => {
    event.preventDefault();
    if (busy) return;
    if (signingUp) {
      for (const input of [firstName, lastName]) {
        input.setCustomValidity(input.value.trim() ? '' : 'Bu maydonni to‘ldiring.');
      }
    }
    if (!form.reportValidity()) return;
    if (new TextEncoder().encode(password.value).length > 72) {
      message('Parol juda uzun. Qisqaroq parol kiriting.'); return;
    }
    const data = { email: email.value.trim(), password: password.value };
    if (signingUp) Object.assign(data, { firstName: firstName.value.trim(), lastName: lastName.value.trim() });
    message('');
    loading(true);
    try {
      const result = await request(signingUp ? '/api/auth/signup' : '/api/auth/signin', data);
      if (!result.ok) failure(result);
      else if (signingUp) {
        setMode(false);
        history.replaceState(null, '', '/login.html');
        message('Hisob yaratildi. Emailingizga yuborilgan havola orqali manzilingizni tasdiqlang, so‘ng kiring. Spam papkasini ham tekshiring.', 'success');
        password.focus();
      } else complete(result.body);
    } catch (error) { networkError(error); }
    finally { loading(false); }
  });
  resend.addEventListener('click', async () => {
    if (busy) return;
    if (!email.reportValidity()) { email.focus(); return; }
    message('');
    loading(true, 'Xat yuborilmoqda…');
    try {
      const result = await request('/api/auth/resend-verification', { email: email.value.trim() });
      if (result.ok) message('Agar hisobingiz tasdiqlanmagan bo‘lsa, tasdiqlash xati yuboriladi. Spam papkasini ham tekshiring.', 'success');
      else failure(result);
    } catch (error) { networkError(error); }
    finally { loading(false); }
  });
  google.addEventListener('click', event => { if (busy) event.preventDefault(); });
  document.getElementById('switch-account').addEventListener('click', () => {
    try { sessionStorage.removeItem(tokenKey); } catch { /* Storage can be disabled by the browser. */ }
    form.reset();
    success.hidden = true;
    controls.hidden = false;
    setMode(false);
    history.replaceState(null, '', '/login.html');
    email.focus();
  });

  const params = new URLSearchParams(location.search);
  const googleResult = params.get('google');
  setMode(!googleResult && params.get('mode') === 'signup');
  if (googleResult) history.replaceState(null, '', location.pathname);
  if (googleResult === 'error') message('Google orqali kirish bekor qilindi yoki amalga oshmadi. Qayta urinib ko‘ring.');
  if (googleResult === 'callback') {
    loading(true, 'Google orqali kirilmoqda…');
    message('Google hisobingiz tekshirilmoqda…', 'info');
    request('/api/auth/google/callback')
      .then(result => result.ok ? complete(result.body) : failure(result, true))
      .catch(networkError).finally(() => loading(false));
  }
})();
