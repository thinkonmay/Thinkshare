cmake_minimum_required(VERSION 2.20.0)
project(Slave C)


# add_compile_options (-fdiagnostics-color=always)
# add_compile_options (warning_level=1)

if (MINGW)
    set(THINKMAY_DIR $ENV{THINKMAY_DIR})
    set(CMAKE_PREFIX_PATH ${THINKMAY_DIR}/gstreamer/mingw_x86)
    set(INCLUDE_LIBRARY ${THINKMAY_DIR}/gstreamer/mingw_x86/lib)

    foreach( OUTPUTCONFIG ${CMAKE_CONFIGURATION_TYPES} )
        string( TOUPPER ${OUTPUTCONFIG} OUTPUTCONFIG )
        set( CMAKE_ARCHIVE_OUTPUT_DIRECTORY_${OUTPUTCONFIG} ${CMAKE_SOURCE_DIR}/../Deployment/app-packetizer/bin )
        set( CMAKE_LIBRARY_OUTPUT_DIRECTORY_${OUTPUTCONFIG} ${CMAKE_SOURCE_DIR}/../Deployment/app-packetizer/bin )
        set( CMAKE_RUNTIME_OUTPUT_DIRECTORY_${OUTPUTCONFIG} ${CMAKE_SOURCE_DIR}/../Deployment/app-packetizer/bin )
    endforeach( OUTPUTCONFIG CMAKE_CONFIGURATION_TYPES )
elseif(MSVC)
    set(THINKMAY_DIR $ENV{THINKMAY_DIR})
    set(CMAKE_PREFIX_PATH ${THINKMAY_DIR}/gstreamer/msvc_x86_64)
    set(INCLUDE_LIBRARY ${THINKMAY_DIR}/gstreamer/msvc_x86_64/lib)
    
    foreach( OUTPUTCONFIG ${CMAKE_CONFIGURATION_TYPES} )
        string( TOUPPER ${OUTPUTCONFIG} OUTPUTCONFIG )
        set( CMAKE_ARCHIVE_OUTPUT_DIRECTORY_${OUTPUTCONFIG} ${CMAKE_SOURCE_DIR}/../Deployment/app-packetizer/bin )
        set( CMAKE_LIBRARY_OUTPUT_DIRECTORY_${OUTPUTCONFIG} ${CMAKE_SOURCE_DIR}/../Deployment/app-packetizer/bin )
        set( CMAKE_RUNTIME_OUTPUT_DIRECTORY_${OUTPUTCONFIG} ${CMAKE_SOURCE_DIR}/../Deployment/app-packetizer/bin )
    endforeach( OUTPUTCONFIG CMAKE_CONFIGURATION_TYPES )
endif()




set(SHARED_ITEMS ${CMAKE_SOURCE_DIR}/shared-items/include)

add_subdirectory(shared-items)
add_subdirectory(session-core)
add_subdirectory(agent)
add_subdirectory(file-transfer-service)
add_subdirectory(remote-app)