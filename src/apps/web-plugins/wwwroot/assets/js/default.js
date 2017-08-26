$(".live-template").each(function(){

    var el = $(this)
    el.find("textarea").on("input", function(){

        var request = { template: el.find("textarea").val() }

        $.ajax({
            type: "POST",
            url: "/template/eval" + location.search,
            data: JSON.stringify(request),
            contentType: "application/json",
            dataType: "html"
        }).done(function(data){
            el.removeClass('error').find(".output").html(data)
        }).fail(function(jqxhr){ handleError(el, jqxhr) })
    })
    .trigger("input")

})

function handleError(el, jqxhr) {
    try {
        console.log('template error:', jqxhr.status, jqxhr.statusText)
        el.addClass('error')
        var errorResponse = JSON.parse(jqxhr.responseText);
        var status = errorResponse.responseStatus;
        if (status) {
            el.find('.output').html('<div class="alert alert-danger"><pre>' + status.errorCode + ' ' + status.message +
             '\n\nStackTrace:\n' + status.stackTrace + '</pre></div>')
        }
    } catch(e) {
        el.find('.output').html('<div class="alert alert-danger"><pre>' + jqxhr.status + ' ' + jqxhr.statusText + '</pre></div>')
    }
}
