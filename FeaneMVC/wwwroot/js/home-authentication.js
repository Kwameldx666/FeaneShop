(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const form = document.getElementById('registerForm');
        if (!form) {
            return;
        }

        form.addEventListener('submit', function (event) {
            var el = form;
            var name = el.username.value;
            var email = el.email.value;
            var password = el.password.value;

            var errorMessageName = '';
            var errorMessageEmail = '';
            var errorMessagePassword = '';

            if (name.length === 0) {
                errorMessageName += 'Пожалуйста заполните поле имени.<br>';
            }

            if (email.length === 0) {
                errorMessageEmail += 'Пожалуйста заполните поле email!<br>';
            }

            if (password.length === 0) {
                errorMessagePassword += 'Пожалуйста введите пароль.<br>';
            }

            if (name.length > 0 && name.length < 4) {
                errorMessageName += 'Некорректная длина имени (минимум 4 символа).<br>';
            }

            if (name.length > 20) {
                errorMessageName += 'Некорректная длина имени (максимум 20 символов).<br>';
            }

            if (password.length > 0 && password.length < 8) {
                errorMessagePassword += 'Слишком короткий пароль (минимум 8 символов).<br>';
            } else if (password.length >= 8) {
                if (!/[A-Z]/.test(password)) {
                    errorMessagePassword += 'Пароль должен содержать хотя бы одну заглавную букву.<br>';
                }
                if (!/[0-9]/.test(password)) {
                    errorMessagePassword += 'Пароль должен содержать хотя бы одну цифру.<br>';
                }
            }

            if (errorMessageName !== '') {
                document.getElementById('errorName').innerHTML = errorMessageName;
                event.preventDefault();
            }

            if (errorMessageEmail !== '') {
                document.getElementById('errorEmail').innerHTML = errorMessageEmail;
                event.preventDefault();
            }

            if (errorMessagePassword !== '') {
                document.getElementById('errorPassword').innerHTML = errorMessagePassword;
                event.preventDefault();
            }
        });
    });
})();
