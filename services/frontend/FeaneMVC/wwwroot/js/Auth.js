(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const container = document.getElementById('container');
        const signUpButton = document.getElementById('signUp');
        const signInButton = document.getElementById('signIn');

        if (signUpButton && container) {
            signUpButton.addEventListener('click', function () {
                container.classList.add('right-panel-active');
            });
        }

        if (signInButton && container) {
            signInButton.addEventListener('click', function () {
                container.classList.remove('right-panel-active');
            });
        }

        const contactForm = document.getElementById('contactForm');
        if (!contactForm) {
            return;
        }

        contactForm.addEventListener('submit', function (event) {
            const name = contactForm.username?.value ?? '';
            const email = contactForm.email?.value ?? '';
            const password = contactForm.password?.value ?? '';
            const credential = contactForm.credential?.value ?? '';

            let errorMessageName = '';
            let errorMessageEmail = '';
            let errorMessagePassword = '';
            let errorMessageCredential = '';

            if (credential.length === 0) {
                errorMessageCredential += 'Пожалуйста заполните поле логина.<br>';
            }

            if (name.length === 0) {
                errorMessageName += 'Пожалуйста заполните поле имени.<br>';
            } else if (name.length < 4) {
                errorMessageName += 'Некорректная длина имени (минимум 4 символа).<br>';
            } else if (name.length > 20) {
                errorMessageName += 'Некорректная длина имени (максимум 20 символов).<br>';
            }

            if (email.length === 0) {
                errorMessageEmail += 'Пожалуйста заполните поле email!<br>';
            }

            if (password.length === 0) {
                errorMessagePassword += 'Пожалуйста введите пароль.<br>';
            } else {
                if (password.length < 8) {
                    errorMessagePassword += 'Слишком короткий пароль (минимум 8 символов).<br>';
                }

                if (!/[A-Z]/.test(password)) {
                    errorMessagePassword += 'Пароль должен содержать хотя бы одну заглавную букву.<br>';
                }

                if (!/[0-9]/.test(password)) {
                    errorMessagePassword += 'Пароль должен содержать хотя бы одну цифру.<br>';
                }
            }

            const credentialErrorElement = document.getElementById('errorCredential');
            const nameErrorElement = document.getElementById('errorName');
            const emailErrorElement = document.getElementById('errorEmail');
            const passwordErrorElement = document.getElementById('errorPassword');

            if (credentialErrorElement) {
                credentialErrorElement.innerHTML = errorMessageCredential;
            }
            if (nameErrorElement) {
                nameErrorElement.innerHTML = errorMessageName;
            }
            if (emailErrorElement) {
                emailErrorElement.innerHTML = errorMessageEmail;
            }
            if (passwordErrorElement) {
                passwordErrorElement.innerHTML = errorMessagePassword;
            }

            if (errorMessageCredential || errorMessageName || errorMessageEmail || errorMessagePassword) {
                event.preventDefault();
            }
        });
    });
})();
