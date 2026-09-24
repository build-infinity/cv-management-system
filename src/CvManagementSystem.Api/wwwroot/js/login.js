'use strict';
(() => {
  const form = document.getElementById('login-form');
  const email = document.getElementById('email');
  const password = document.getElementById('password');
  const submit = document.getElementById('submit-login');
  const resend = document.getElementById('resend-email');
  const google = document.getElementById('google-login');
  const feedback = document.getElementById('feedback');
  const controls = document.getElementById('auth-controls');
  const success = document.getElementById('success-panel');
  const tokenKey = 'cv.accessToken';
  let busy = false;
  document.getElementById('year').textContent = new Date().getFullYear();

  function message(text, kind = 'error') {
    feedback.textContent = text;
    feedback.className = 'feedback ' + kind;
    feedback.hidden = !text;
  }
  function loading(value, label = 'Kirilmoqda…') {
    busy = value;
    submit.disabled = value;
    resend.disabled = value;
    google.setAttribute('aria-disabled', String(value));
    form.setAttribute('aria-busy', String(value));
    submit.firstElementChild.textContent = value ? label : 'Kirish';
  }
  async function request(url, data) {
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), 30000);
    try {
      const response = await fetch(url, {
        method: data === undefined ? 'GET' : 'POST',
        headers: data === undefined ? { Accept: 'application/json' } : { Accept: 'application/json', 'Content-Type': 'application/json' },
        body: data === undefined ? undefined : JSON.stringify(data),
        credentials: 'same-origin', cache: 'no-store', signal: controller.signal
      });
      let body = null;
      try { body = await response.json(); } catch { /* Handle non-JSON server errors below. */ }
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
    password.value = '';
    message('');
    controls.hidden = true;
    success.hidden = false;
    document.getElementById('login-subtitle').textContent = 'Kirish muvaffaqiyatli yakunlandi.';
    success.focus();
  }
  function failure(result, isGoogle = false) {
    if (result.status === 403) message('Emailingiz hali tasdiqlanmagan. Xatingizdagi linkni oching yoki tasdiqlash xatini qayta yuboring.');
    else if (result.status === 401) message(isGoogle ? 'Google orqali kirish yakunlanmadi. Qayta urinib ko‘ring.' : 'Email yoki parol noto‘g‘ri. Tekshirib, qayta kiriting.');
    else if (result.status === 409) message('Bu email bilan hisob mavjud. Email va parolingiz orqali kiring.');
    else if (result.status === 429) message('So‘rovlar juda ko‘p. Birozdan keyin qayta urinib ko‘ring.');
    else if (result.status === 400) message('Kiritilgan ma’lumotlarni tekshiring va qayta urinib ko‘ring.');
    else message('Server bilan bog‘lanishda muammo yuz berdi. Birozdan keyin qayta urinib ko‘ring.');
  }
  function networkError(error) {
    message(error.name === 'AbortError' ? 'So‘rov vaqti tugadi. Qayta urinib ko‘ring.' : 'Ulanishni tekshiring va qayta urinib ko‘ring.');
  }
  document.getElementById('toggle-password').addEventListener('click', event => {
    const visible = password.type === 'password';
    password.type = visible ? 'text' : 'password';
    event.currentTarget.setAttribute('aria-pressed', String(visible));
    event.currentTarget.setAttribute('aria-label', visible ? 'Parolni yashirish' : 'Parolni ko‘rsatish');
  });
  form.addEventListener('submit', async event => {
    event.preventDefault();
    if (busy || !form.reportValidity()) return;
    if (new TextEncoder().encode(password.value).length > 72) {
      message('Parol juda uzun. Qisqaroq parol kiriting.'); return;
    }
    message(''); loading(true);
    try {
      const result = await request('/api/auth/signin', { email: email.value.trim(), password: password.value });
      if (result.ok) complete(result.body); else failure(result);
    } catch (error) { networkError(error); }
    finally { loading(false); }
  });
  resend.addEventListener('click', async () => {
    if (busy) return;
    if (!email.reportValidity()) { email.focus(); return; }
    message(''); loading(true, 'Kutilmoqda…');
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
    success.hidden = true; controls.hidden = false;
    document.getElementById('login-subtitle').textContent = 'Davom etish uchun hisobingizga kiring.';
    email.focus();
  });
  const params = new URLSearchParams(location.search);
  const googleResult = params.get('google');
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
