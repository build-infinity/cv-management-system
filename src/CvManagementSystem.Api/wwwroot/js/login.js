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
    toggle.textContent = 'Show';
    toggle.setAttribute('aria-pressed', 'false');
    toggle.setAttribute('aria-label', 'Show password');
  }

  function setMode(signup) {
    signingUp = signup;
    const label = signup ? 'Sign up' : 'Sign in';
    title.textContent = label;
    document.title = label + ' — CV Workspace';
    subtitle.textContent = signup ? 'Create your account.' : 'Sign in to your account.';
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
    document.getElementById('mode-prompt').textContent = signup ? 'Already have an account?' : 'New here?';
    switchMode.textContent = signup ? 'Sign in' : 'Sign up';
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
    submit.textContent = value ? (label || (signingUp ? 'Creating account…' : 'Signing in…')) : (signingUp ? 'Sign up' : 'Sign in');
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
      message('Could not complete sign-in. Please try again.');
      return;
    }
    try { sessionStorage.setItem(tokenKey, body.accessToken); }
    catch { message('Your browser could not save your session. Check your browser settings.'); return; }
    resetPassword();
    message('');
    controls.hidden = true;
    success.hidden = false;
    title.textContent = 'You’re signed in';
    subtitle.textContent = 'You have signed in successfully.';
    success.focus();
  }

  function failure(result, isGoogle = false) {
    if (result.status === 403) message('Your email is not verified yet. Open the link in your email or resend the verification email.');
    else if (result.status === 401) message(isGoogle ? 'Google sign-in could not be completed. Please try again.' : 'Incorrect email or password. Please try again.');
    else if (result.status === 409) message('An account with this email already exists. Switch to sign in.');
    else if (result.status === 429) message('Too many requests. Please try again later.');
    else if (result.status === 400) message('Check your details and try again.');
    else message('Could not reach the server. Please try again later.');
  }

  function networkError(error) {
    message(error.name === 'AbortError' ? 'The request timed out. Please try again.' : 'Check your connection and try again.');
  }

  for (const input of [firstName, lastName]) {
    input.addEventListener('input', () => input.setCustomValidity(''));
  }
  toggle.addEventListener('click', () => {
    const visible = password.type === 'password';
    password.type = visible ? 'text' : 'password';
    toggle.textContent = visible ? 'Hide' : 'Show';
    toggle.setAttribute('aria-pressed', String(visible));
    toggle.setAttribute('aria-label', visible ? 'Hide password' : 'Show password');
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
        input.setCustomValidity(input.value.trim() ? '' : 'Please fill out this field.');
      }
    }
    if (!form.reportValidity()) return;
    if (new TextEncoder().encode(password.value).length > 72) {
      message('This password is too long. Please use a shorter password.'); return;
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
        message('Account created. Open the verification link in your email, then sign in. Check your spam folder too.', 'success');
        password.focus();
      } else complete(result.body);
    } catch (error) { networkError(error); }
    finally { loading(false); }
  });
  resend.addEventListener('click', async () => {
    if (busy) return;
    if (!email.reportValidity()) { email.focus(); return; }
    message('');
    loading(true, 'Sending email…');
    try {
      const result = await request('/api/auth/resend-verification', { email: email.value.trim() });
      if (result.ok) message('If your account needs verification, an email will be sent. Check your spam folder too.', 'success');
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
  if (googleResult === 'error') message('Google sign-in was cancelled or could not be completed. Please try again.');
  if (googleResult === 'callback') {
    loading(true, 'Signing in with Google…');
    message('Checking your Google account…', 'info');
    request('/api/auth/google/callback')
      .then(result => result.ok ? complete(result.body) : failure(result, true))
      .catch(networkError).finally(() => loading(false));
  }
})();
