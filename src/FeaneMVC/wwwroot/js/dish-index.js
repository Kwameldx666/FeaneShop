(function () {
    const container = document.querySelector('.admin-container');
    if (!container) {
        return;
    }

    const deleteDishUrl = container.dataset.deleteUrl;
    const feedback = document.getElementById('dish-feedback');

    function showFeedback(message, type) {
        if (!feedback) {
            alert(message);
            return;
        }

        feedback.textContent = message;
        feedback.classList.remove('d-none', 'success-alert', 'error-alert', 'alert-success', 'alert-danger');
        if (type === 'success') {
            feedback.classList.add('success-alert', 'alert-success');
        } else {
            feedback.classList.add('error-alert', 'alert-danger');
        }
    }

    function handleJsonResponse(response) {
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

            return data || { status: false, message: 'Unexpected server response.' };
        });
    }

    window.deleteDish = function (id) {
        if (!deleteDishUrl) {
            console.error('Delete dish URL is not defined.');
            return;
        }

        if (!confirm('Are you sure you want to delete this dish?')) {
            return;
        }

        const formData = new URLSearchParams();
        formData.append('id', id);

        fetch(deleteDishUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: formData.toString()
        })
            .then(handleJsonResponse)
            .then(function (response) {
                if (response && response.status) {
                    const row = document.getElementById('dish-row-' + id);
                    if (row) {
                        row.remove();
                    }
                    showFeedback(response.message || 'Dish deleted successfully.', 'success');
                } else {
                    showFeedback((response && response.message) || 'Failed to delete the dish. Please try again.', 'error');
                }
            })
            .catch(function (error) {
                showFeedback('An error occurred: ' + error.message, 'error');
            });
    };
})();
