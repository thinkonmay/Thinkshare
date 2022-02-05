// utilities
var get = function (selector, scope) {
  scope = scope ? scope : document;
  return scope.querySelector(selector);
};

var getAll = function (selector, scope) {
  scope = scope ? scope : document;
  return scope.querySelectorAll(selector);
};

const run = 
`agent.exe

Enter your thinkmay manager username:
[USERNAME]: this_is_my_thinkmay_manager_user

Enter your thinkmay manager password:
[PASSWORD]: this_is_my_thinkmay_manager_password

thinkmay cluster manager ip:
[IP ADDRESS]: this is_my_cluster_manager_url

login success
Register successfully with cluster manager and got worker token `;

const build = 
`./build.bat

** Visual Studio 2022 Developer Command Prompt v17.0.0-pre.5.0
** Copyright (c) 2021 Microsoft Corporation

[vcvarsall.bat] Environment initialized for: 'x64_x86'
-- Selecting Windows SDK version 10.0.20348.0 to target Windows 10.0.21996.
-- Configuring done
-- Generating done
-- Build files have been written to: C:/Users/huyho/Desktop/pcc/public/personal-cloud-computing/build
Microsoft (R) Build Engine version 17.0.0-preview-21480-03+bbcce1dff for .NET Framework
Copyright (C) Microsoft Corporation. All rights reserved.


Build succeeded.
  0 Warning(s)
  0 Error(s)

Time Elapsed 00:00:00.85./build.bat `;

const host = 
`cd ./worker/cluster
sudo docker-compose up

Starting cluster_ui               ... done
Starting redis_cache              ... done
Starting cluster_database_manager ... done
Starting cluster_database         ... done
Starting cluster_manager          ... done
`

const test = 
`[RUN SIGNALLING TEST SERVER]
cd ./testing/Signalling
dotnet run .                    

[11:47:46 INF] Now listening on: http://localhost:5000
[11:47:46 INF] Now listening on: https://localhost:5001
[11:47:46 INF] Application started. Press Ctrl+C to shut down.
[11:47:46 INF] Hosting environment: Production

[RUN SESSION CORE]
.\\bin\\remote-app.exe

Starting development client
Starting remote app with remote token TestingClientModuleToken
Fail to get turn server, setting default value

[RUN REMOTE APP]
.\\bin\\session-core.exe

Starting in development environment
`

const demo_code = 
{
  run:  run,
  build: build,
  host: host, 
  test: test
}


var done = false;
async function
display_demo_text(txt)
{
  // setup typewriter effect in the terminal demo
  done = true;
  var i = 0;

  var speed = 0;
  await (new Promise(resolve => setTimeout(resolve,500)));
  document.getElementById('demo').innerHTML = "";
  function typeItOut () {
    if (i < txt.length) {
      if(done == true)
      {
        return;
      }
      document.getElementById('demo').innerHTML += txt.charAt(i);
      i++;
      setTimeout(typeItOut, speed);
    }
  }
  done = false;
  setTimeout(typeItOut, 500);
}

display_demo_text(demo_code["run"]);

// document.getElementById('demo')[0].innerHTML = "";


// toggle tabs on codeblock
window.addEventListener("load", async function() {
  // get all tab_containers in the document
  var tabContainers = getAll(".tab__container");

  get('.tab__menu', tabContainers[0]).addEventListener("click", demoCodeClick);

  function demoCodeClick (event) {
    var scope = event.currentTarget.parentNode;
    var clickedTab = event.target;
    var tabs = getAll('.tab', scope);
    var activeCode = clickedTab.innerHTML;

    display_demo_text(demo_code[activeCode]);

    // remove all active tab classes
    for (var i = 0; i < tabs.length; i++) {
      tabs[i].classList.remove('active');
    }

    // apply active classes on desired tab and pane
    clickedTab.classList.add('active');
  }




  // bind click event to each tab container
  for (var i = 1; i < tabContainers.length; i++) {
    get('.tab__menu', tabContainers[i]).addEventListener("click", tabClick);
  }

  // each click event is scoped to the tab_container
  function tabClick (event) {
    var scope = event.currentTarget.parentNode;
    var clickedTab = event.target;
    var tabs = getAll('.tab', scope);
    var panes = getAll('.tab__pane', scope);
    var activePane = get(`.${clickedTab.getAttribute('data-tab')}`, scope);

    // remove all active tab classes
    for (var i = 0; i < tabs.length; i++) {
      tabs[i].classList.remove('active');
    }

    // remove all active pane classes
    for (var i = 0; i < panes.length; i++) {
      panes[i].classList.remove('active');
    }

    // apply active classes on desired tab and pane
    clickedTab.classList.add('active');
    activePane.classList.add('active');
  }
});

//in page scrolling for documentaiton page
var btns = getAll('.js-btn');
var sections = getAll('.js-section');

function setActiveLink(event) {
  // remove all active tab classes
  for (var i = 0; i < btns.length; i++) {
    btns[i].classList.remove('selected');
  }

  event.target.classList.add('selected');
}

function smoothScrollTo(i, event) {
  var element = sections[i];
  setActiveLink(event);

  window.scrollTo({
    'behavior': 'smooth',
    'top': element.offsetTop - 20,
    'left': 0
  });
}

if (btns.length && sections.length > 0) {
  for (var i = 0; i<btns.length; i++) {
    btns[i].addEventListener('click', smoothScrollTo.bind(this,i));
  }
}

// fix menu to page-top once user starts scrolling
window.addEventListener('scroll', function () {
  var docNav = get('.doc__nav > ul');

  if( docNav) {
    if (window.pageYOffset > 63) {
      docNav.classList.add('fixed');
    } else {
      docNav.classList.remove('fixed');
    }
  }
});

// responsive navigation
var topNav = get('.menu');
var icon = get('.toggle');

window.addEventListener('load', function(){
  function showNav() {
    if (topNav.className === 'menu') {
      topNav.className += ' responsive';
      icon.className += ' open';
    } else {
      topNav.className = 'menu';
      icon.classList.remove('open');
    }
  }
  icon.addEventListener('click', showNav);
});

