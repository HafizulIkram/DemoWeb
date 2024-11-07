function sendAjaxRequest(url, data, token) {
    $.ajax({
        type: 'POST',
        url: url,
        data: data,
        headers: {
            'RequestVerificationToken': token 
        }
    })
        .done((data) => {

            if (data.redirectUrl != null) {
                window.location.href = data.redirectUrl;
            }

            else {
                $('.modal-body').text(data.message);

                $('#exampleModal').modal('show');
            }
         
        })
        .fail((err) => {
            $('.modal-body').text("Error had occured " + err);

            $('#exampleModal').modal('show');
         
        })
        .always(() => {
            console.log('always called');
        });
}

