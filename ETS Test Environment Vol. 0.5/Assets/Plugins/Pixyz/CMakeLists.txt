
#install(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}/Packages DESTINATION unity)

if(${PXZ_OS} STREQUAL MACOS)
   install(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} DESTINATION .)

set(pxzVersion ${EXECUTABLE_OUTPUT_PATH}/Release/pixyz_version)

STRING(REGEX REPLACE "\\" "\/" UNITY_PATH ${UNITY_PATH} )

install(
   CODE 
   "
    message(\"Launch pixyz_version.exe\")
    message(\"${pxzVersion} +%M.%s.%u.%r\")
    execute_process(
       COMMAND \"${pxzVersion}\" +%M.%s.%u.%r
       OUTPUT_VARIABLE pxz_version
    )
    message(\"\${pxz_version}\")
    string(REPLACE \"\\n\" \"\" pxz_version \${pxz_version})
    string(REPLACE \"\\r\" \"\" pxz_version \${pxz_version})
    
    file(REMOVE_RECURSE \"${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins/Pixyz/Binaries\" )
    file(MAKE_DIRECTORY \"${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins/Pixyz/Binaries\" )

    execute_process(
      COMMAND sh -c \"cp -Rf ${CMAKE_INSTALL_PREFIX}/bin/* ${CMAKE_INSTALL_PREFIX}/PixyzPlugin4Unity.bundle/Contents/MacOS/ && sync \"
      OUTPUT_VARIABLE output
    )
    message(\"\${output}\")
    execute_process(
      COMMAND sh -c \"cp -Rf ${CMAKE_INSTALL_PREFIX}/PixyzPluginUnity.dll ${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins/Pixyz/Binaries/ && sync \"
      OUTPUT_VARIABLE output
    )
    message(\"\${output}\")
    execute_process(
      COMMAND sh -c \"cp -Rf ${CMAKE_INSTALL_PREFIX}/PixyzPluginUnity.dll.meta ${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins/Pixyz/Binaries/ && sync \"
      OUTPUT_VARIABLE output
    )
    message(\"\${output}\")
    execute_process(
      COMMAND sh -c \"cp -Rf ${CMAKE_INSTALL_PREFIX}/PiXYZPlugin4Unity.bundle ${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins/Pixyz/Binaries/ && sync \"
      OUTPUT_VARIABLE output
    )
    message(\"\${output}\")
    execute_process(
      COMMAND sh -c \"${UNITY_PATH}/Contents/MacOS/Unity -quit -batchmode -logFile ${CMAKE_INSTALL_PREFIX}/log.txt -nographics -projectpath ${CMAKE_INSTALL_PREFIX}/unity -exportPackage Assets/Plugins/Pixyz ${CMAKE_INSTALL_PREFIX}/PiXYZ4Unity\${pxz_version}-osx.unitypackage\"
      OUTPUT_VARIABLE output
    )
    message(\"\${output}\")

   file(READ \"${CMAKE_INSTALL_PREFIX}/log.txt\" build_unity)
   message(\"\${build_unity}\")

   "
)
endif()


if(${PXZ_OS} STREQUAL WIN32)
   install(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} DESTINATION .)

   if(PXZ_COMPILER MATCHES "MSC" AND NOT CMAKE_MAKE_PROGRAM MATCHES "jom")
      set(pxzVersion ${EXECUTABLE_OUTPUT_PATH}/Release/pixyz_version.exe)
      set(pxzConfusor ${EXECUTABLE_OUTPUT_PATH}/Release/PiXYZConfusorGenerator.exe)
   else()
      set(pxzVersion ${EXECUTABLE_OUTPUT_PATH}/pixyz_version.exe)
      set(pxzConfusor ${EXECUTABLE_OUTPUT_PATH}/Release/PiXYZConfusorGenerator.exe)
   endif()
   
   set(install_dir "${CMAKE_INSTALL_PREFIX}")
   
   if(NOT IS_ABSOLUTE ${install_dir})
      get_filename_component(install_dir ${CMAKE_BINARY_DIR}/${CMAKE_INSTALL_PREFIX} REALPATH)
   endif()
   message("${install_dir}")
   
install(
   CODE 
   "
   file(REMOVE_RECURSE \"${CMAKE_INSTALL_PREFIX}/bin\")
   file(RENAME \"${CMAKE_INSTALL_PREFIX}\" \"${CMAKE_INSTALL_PREFIX}/../tmpBin\")
   file(MAKE_DIRECTORY \"${CMAKE_INSTALL_PREFIX}\")
   file(RENAME \"${CMAKE_INSTALL_PREFIX}/../tmpBin\" \"${CMAKE_INSTALL_PREFIX}/bin\")
   file(RENAME \"${CMAKE_INSTALL_PREFIX}/bin/unity\" \"${CMAKE_INSTALL_PREFIX}/unity\")
   file(REMOVE_RECURSE \"${CMAKE_INSTALL_PREFIX}/unity/Binaries\")
   file(MAKE_DIRECTORY \"${CMAKE_INSTALL_PREFIX}/unity/Binaries\")
   
   message(\"Executing code\")
   message(\"Launch PiXYZConfusorGenerator on PixyzPluginUnity.crproj\")
   execute_process(
   COMMAND \"${pxzConfusor}\" \"${CMAKE_INSTALL_PREFIX}/PixyzPluginUnity.crproj\" PixyzPluginUnity.dll \"${install_dir}\" \"${install_dir}/bin\" \"${UNITY_PATH}/Editor/Data/Managed\"
   OUTPUT_VARIABLE commandOutput
   )
   message(\"\${commandOutput}\")
   message(\"Launch Confuser.CLI.exe on PixyzPluginUnity.crproj\")
   execute_process(
   COMMAND \"${PXZ_DEPENDENCIES_DIR}/confuser/Confuser.CLI.exe\" \"${CMAKE_INSTALL_PREFIX}/PixyzPluginUnity.crproj\"
   OUTPUT_VARIABLE commandOutput
   )
   message(\"\${commandOutput}\")
   message(\"Launch pixyz_version.exe\")
   message(\"${pxzVersion} +%M.%s.%u.%r\")
   execute_process(
   COMMAND \"${pxzVersion}\" +%M.%s.%u.%r
   OUTPUT_VARIABLE pxz_version
   )
   message(\"\${pxz_version}\")
   string(REPLACE \"\\n\" \"\" pxz_version \${pxz_version})
   string(REPLACE \"\\r\" \"\" pxz_version \${pxz_version})
   
   message(\"Launch copy files\")
   file(RENAME \"${CMAKE_INSTALL_PREFIX}/bin\" \"${CMAKE_INSTALL_PREFIX}/unity/Binaries/native\" )
   file(REMOVE \"${CMAKE_INSTALL_PREFIX}/unity/Binaries/native/PixyzPluginUnity.dll\" )
   file(REMOVE \"${CMAKE_INSTALL_PREFIX}/unity/Binaries/native/PiXYZPlugin4Unity.lib\" )
   file(COPY \"${CMAKE_INSTALL_PREFIX}/PixyzPluginUnity.dll\" DESTINATION \"${CMAKE_INSTALL_PREFIX}/unity/Binaries\" )
   
   message(\"Creating Unity Package\")
   execute_process(
      COMMAND \"${PXZ_DEPENDENCIES_DIR}/unitypacker/UnityPacker.exe\" mode pack folder \"${CMAKE_INSTALL_PREFIX}/unity\" package \"${CMAKE_INSTALL_PREFIX}/PixyzPluginUnity-\${pxz_version}-win64\" root \"Assets/Plugins/Pixyz\" ignore \"^(.+\\.unitypackage|.+\\.cmd|.+\\.obj|CMakeLists\\.txt|.+\\.hdr|package\\.json.*)\$\"
   )

   message(\"Done !\")
   #file(REMOVE_RECURSE \"${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins/Pixyz/Binaries\" )
   "
)
endif()

if(${PXZ_OS} STREQUAL LINUX)
set(pxzVersion ${EXECUTABLE_OUTPUT_PATH}/pixyz_version)

install(
   CODE
   "
   message(\"Launch pixyz_version.exe\")
   message(\"${pxzVersion} +%M.%s.%u.%r\")
   execute_process(
      COMMAND \"${pxzVersion}\" +%M.%s.%u.%r
      OUTPUT_VARIABLE pxz_version
   )
   message(\"\${pxz_version}\")
   string(REPLACE \"\\n\" \"\" pxz_version \${pxz_version})
   string(REPLACE \"\\r\" \"\" pxz_version \${pxz_version})

   message(\"Launch copy files\")
   file(REMOVE_RECURSE ${CMAKE_INSTALL_PREFIX}/bin)
   file(RENAME ${CMAKE_INSTALL_PREFIX} ${CMAKE_INSTALL_PREFIX}/../tmpBin)
   file(MAKE_DIRECTORY  ${CMAKE_INSTALL_PREFIX})
   file(RENAME ${CMAKE_INSTALL_PREFIX}/../tmpBin ${CMAKE_INSTALL_PREFIX}/bin)

   message(\"Creating Unity Package\")
   #execute_process(
   #   COMMAND sh -c \"${UNITY_PATH}/Unity -quit -batchmode -nographics -manualLicenseFile ${CMAKE_BUILD_DIR}/Unity_v2019.x.ulf\"
   #   OUTPUT_VARIABLE commandOutput
   # )
   #message(\"\${commandOutput}\")

   execute_process(
      COMMAND bash -c \"TERM=xterm && ${UNITY_PATH}/Unity -quit -batchmode -logFile ${CMAKE_INSTALL_PREFIX}/create_log.txt -nographics -createProject ${CMAKE_INSTALL_PREFIX}/unity\"
      OUTPUT_VARIABLE commandOutput
   )
   message(\"\${commandOutput}\")

   file(REMOVE_RECURSE ${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins)
   file(MAKE_DIRECTORY ${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins)
   file(COPY ${CMAKE_CURRENT_SOURCE_DIR} DESTINATION ${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins)
   file(RENAME ${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins/unity ${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins/Pixyz )
   file(MAKE_DIRECTORY ${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins/Pixyz/Binaries)

   file(RENAME ${CMAKE_INSTALL_PREFIX}/bin ${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins/Pixyz/Binaries/native )
   file(RENAME ${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins/Pixyz/Binaries/native/PixyzPluginUnity.dll ${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins/Pixyz/Binaries/PixyzPluginUnity.dll )
   file(RENAME ${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins/Pixyz/Binaries/native/PixyzPluginUnity.dll.meta ${CMAKE_INSTALL_PREFIX}/unity/Assets/Plugins/Pixyz/Binaries/PixyzPluginUnity.dll.meta )

   message(\"${UNITY_PATH}/Unity -quit -batchmode -logFile ${CMAKE_INSTALL_PREFIX}/log.txt -nographics -projectpath ${CMAKE_INSTALL_PREFIX}/unity -exportPackage Assets/Plugins/Pixyz ${CMAKE_INSTALL_PREFIX}/PixyzPluginUnity-\${pxz_version}-linux-amd64.unitypackage\")

   execute_process(
      COMMAND bash -c \"TERM=xterm && cd ${CMAKE_INSTALL_PREFIX} && ${UNITY_PATH}/Unity -quit -batchmode -logFile ${CMAKE_INSTALL_PREFIX}/build_log.txt -nographics -projectpath ${CMAKE_INSTALL_PREFIX}/unity -exportPackage Assets/Plugins/Pixyz ${CMAKE_INSTALL_PREFIX}/PiXYZ4Unity-\${pxz_version}-linux-amd64.unitypackage\"
      OUTPUT_VARIABLE commandOutput
   )
   message(\"\${commandOutput}\")
   message(\"Done !\")
   "
)
endif()
