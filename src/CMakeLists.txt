cmake_minimum_required(VERSION 2.6)
project(Aastra)

if(NOT SpicaInstall)
    set(SpicaInstall ${CMAKE_SOURCE_DIR})
endif(NOT SpicaInstall)

set(Aastra_CSFLAGS
    "-optimize+"
    "-nowarn:0162,0219"
    "-debug"
    "-keyfile:${CMAKE_CURRENT_SOURCE_DIR}/aastra.snk")
set(Aastra_OUT
    "${CMAKE_CURRENT_BINARY_DIR}/aastra.exe")
set(Aastra_LIBS
    "-r:${CMAKE_CURRENT_SOURCE_DIR}/../lib/Antlr3.Runtime.dll"
    "-r:${CMAKE_CURRENT_SOURCE_DIR}/../lib/StringTemplate.dll"
    "-r:${SpicaInstall}/bin/Spica.Castor.dll")
set(Aastra_CP
    "${CMAKE_CURRENT_SOURCE_DIR}/../lib/antlr-2.7.7.jar:${CMAKE_CURRENT_SOURCE_DIR}/../lib/antlr-3.1.3.jar:${CMAKE_CURRENT_SOURCE_DIR}/../lib/stringtemplate-3.2.jar")

set(AastraDump_OUT
    "${CMAKE_CURRENT_BINARY_DIR}/aastradump.exe")

set(AastraDump_LIBS
    "-r:${CMAKE_CURRENT_SOURCE_DIR}/../lib/Antlr3.Utility.dll")

# SpicaML grammar settings
set(Aastra_GRAMMAR "SpicaML")
set(Aastra_GRAMMAR_FILE "${CMAKE_CURRENT_SOURCE_DIR}/${Aastra_GRAMMAR}.g")
set(Aastra_GRAMMAR_SRC
    "${CMAKE_CURRENT_BINARY_DIR}/${Aastra_GRAMMAR}Lexer.cs"
    "${CMAKE_CURRENT_BINARY_DIR}/${Aastra_GRAMMAR}Parser.cs")

file(GLOB Aastra_SRC *.cs)

# Generate parser
add_custom_command(
    OUTPUT ${Aastra_GRAMMAR}Parser.cs
    DEPENDS ${Aastra_GRAMMAR_FILE}
    COMMAND java -cp ${Aastra_CP} org.antlr.Tool ${Aastra_GRAMMAR_FILE} -o ${CMAKE_CURRENT_BINARY_DIR})

# Compile aastra
add_custom_command(
    OUTPUT ${Aastra_OUT}
    DEPENDS ${Aastra_GRAMMAR}Parser.cs ${Aastra_GRAMMAR_SRC} ${Aastra_SRC}
    COMMAND gmcs
        ${Aastra_CSFLAGS}
        ${Aastra_SRC}
        ${Aastra_GRAMMAR_SRC}
        ${Aastra_LIBS}
        -target:exe
        -out:${Aastra_OUT})

add_custom_target(
    Aastra
    ALL DEPENDS ${Aastra_OUT})

file(GLOB AastraDump_SRC dump/*.cs)

# Compile aastradump
add_custom_command(
    OUTPUT ${AastraDump_OUT}
    DEPENDS ${Aastra_GRAMMAR}Parser.cs ${Aastra_GRAMMAR_SRC} ${AastraDump_SRC}
    COMMAND gmcs
        ${Aastra_CSFLAGS}
        ${AastraDump_SRC}
        ${Aastra_GRAMMAR_SRC}
        ${Aastra_LIBS}
        ${AastraDump_LIBS}
        -target:exe
        -out:${AastraDump_OUT})

add_custom_target(
    AastraDump
    ALL DEPENDS ${AastraDump_OUT})

configure_file(skel.in skel @ONLY)

install(FILES ${CMAKE_BINARY_DIR}/skel DESTINATION ${SpicaInstall}/bin RENAME aastra PERMISSIONS OWNER_WRITE OWNER_READ OWNER_EXECUTE GROUP_READ GROUP_EXECUTE WORLD_READ WORLD_EXECUTE)
install(FILES ${CMAKE_BINARY_DIR}/skel DESTINATION ${SpicaInstall}/bin RENAME aastradump PERMISSIONS OWNER_WRITE OWNER_READ OWNER_EXECUTE GROUP_READ GROUP_EXECUTE WORLD_READ WORLD_EXECUTE)
install(FILES ${Aastra_OUT} ${AastraDump_OUT}
    ${CMAKE_BINARY_DIR}/skel
    "${CMAKE_CURRENT_SOURCE_DIR}/../lib/antlr.runtime.dll"
    "${CMAKE_CURRENT_SOURCE_DIR}/../lib/Antlr3.Runtime.dll"
    "${CMAKE_CURRENT_SOURCE_DIR}/../lib/Antlr3.Utility.dll"
    "${CMAKE_CURRENT_SOURCE_DIR}/../lib/StringTemplate.dll"
    DESTINATION ${SpicaInstall}/bin/)
