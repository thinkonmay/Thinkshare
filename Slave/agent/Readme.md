#Agent
1. how to compile agent.


step1: dependencies.
https://vinuniversity-my.sharepoint.com/:u:/g/personal/20hoang_dh_vinuni_edu_vn/ET7oZAGL_JlFuifhhJCiW4wBdMnyDgP3Buq88hJqqNg5Gg?e=KaSYgL
download and extract to C: folder,

step2: https://cmake.org/download/ 
install cmake

step3: run solution
option 1: run build-vs.bat file, invoke make to generate new visual stdio solution 
(this method is not recommended due to some error of pkg-config while finding package's library),
be careful, first thing this bat file do is cleaning build-vs file

option 2: run visual studio solution directly in build-vs file, this solution's dependencies has been set up.


step 3: set agent as startup project and build. 
