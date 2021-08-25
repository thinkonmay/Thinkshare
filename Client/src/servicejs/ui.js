function loadSlaveModel() {
    for(x=0; x< app.SlaveArray.Length; x++) {
        var slave = document.createElement('div');
        
        slave.className = "row";
        slave.innerHTML = fetchHtmlAsText("../component/SlaveModel.html");;


        document.getElementById('SlaveGroup').appendChild(slave);
    }

    for(x=0; x < app.SessionArray.Length; x++) {
        var session = document.createElement('div');
        
        session.className = "row";
        session.innerHTML = fetchHtmlAsText("../component/SlaveModel.html");;


        document.getElementById('SessionGroup').appendChild(session);
    }
}




function reload()
{
    if(app.LoggedIn === false){
        fetch('pages/service.html').then(function (response) {
            return response.text();
        }).then(function (html) {
            document.getElementById("app").innerHTML = html;

        }).catch(function (err) {
            console.warn('Something went wrong.', err);
        });
    }else{
        if(app.ClientConfig.Role === "Administrator"){
            fetch('pages/admin.html').then(function (response) {
                return response.text();
            }).then(function (html) {
                document.getElementById("app").innerHTML = html;

            }).catch(function (err) {
                console.warn('Something went wrong.', err);
            });
        }else{
            fetch('pages/service.html').then(function (response) {
                return response.text();
            }).then(function (html) {
                document.getElementById("app").innerHTML = html;

            }).catch(function (err) {
                console.warn('Something went wrong.', err);
            });
        }
    }
}
