function autogrow(e) {
    var el = e.target;
    el.style.height = "5px";
    el.style.height = (el.scrollHeight)+"px";
}

var textAreas = document.querySelectorAll("textarea[data-autogrow]");
for (var i = 0; i < textAreas.length; i++) {
    textAreas[i].addEventListener('input', autogrow);        
    autogrow({ target: textAreas[i] });
}

function livepreview(e) {
    var el  = e.target;
    var sel = el.getAttribute("data-livepreview");
    
    var formData = new FormData();
    formData.append('content', el.value);

    fetch("/preview", {
        method: 'post',
        body: formData,
    }).then(function(r){
        return r.text();
    }).then(function(r) {
        document.querySelector(sel).innerHTML = r;
    });
}

textAreas = document.querySelectorAll("textarea[data-livepreview]");
for (var i = 0; i < textAreas.length; i++) {
    textAreas[i].addEventListener('input', livepreview);
    livepreview({ target: textAreas[i] });
}
