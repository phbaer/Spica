cmake_minimum_required(VERSION 2.6)
project(Spica)

set(SpicaInstall ${CMAKE_SOURCE_DIR})

add_subdirectory(Castor/CastorSharp)
add_subdirectory(src)

