(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const form = document.querySelector('.contact-form');
        const feedback = document.getElementById('contacts-feedback');
        if (!form) {
            return;
        }

        function showFeedback(message, type) {
            if (!feedback) {
                alert(message);
                return;
            }

            feedback.textContent = message;
            feedback.classList.remove('d-none', 'alert-success', 'alert-danger');
            feedback.classList.add(type === 'success' ? 'alert-success' : 'alert-danger');
        }

        function handleResponse(response) {
            return response.text().then(function (text) {
                var data = null;
                if (text) {
                    try {
                        data = JSON.parse(text);
                    } catch (error) {
                        data = null;
                    }
                }

                if (!response.ok) {
                    var message = (data && data.message) || text || response.statusText;
                    throw new Error(message);
                }

                return data || { success: false, message: 'Unexpected server response.' };
            });
        }

        form.addEventListener('submit', function (event) {
            event.preventDefault();

            const formData = new FormData(form);
            const params = new URLSearchParams();
            formData.forEach(function (value, key) {
                params.append(key, value.toString());
            });

            fetch(form.action, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: params.toString()
            })
                .then(handleResponse)
                .then(function (data) {
                    if (data && data.success) {
                        showFeedback(data.message || 'Contact information updated successfully.', 'success');
                    } else {
                        showFeedback((data && data.message) || 'Failed to update contact information.', 'error');
                    }
                })
                .catch(function (error) {
                    showFeedback('An error occurred: ' + error.message, 'error');
                });
        });
    });
})();
