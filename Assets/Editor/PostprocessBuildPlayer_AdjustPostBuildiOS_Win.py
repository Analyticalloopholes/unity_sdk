#!/usr/bin/env python

import sys
import re
from subprocess import Popen, PIPE
import argparse

from mod_pbxproj import XcodeProject

def main():
    parser = argparse.ArgumentParser(description="Adjust post build iOS script")
    parser.add_argument('ios_project_path', help="path to the folder of the iOS project generated by unity3d")
    
    with open('AdjustPostBuildiOSLog.txt','w') as fileLog:
        # log function with file injected
        LogFunc = LogInput(fileLog)
       
        # path of the Xcode SDK on the system
        xcode_sdk_path = get_xcode_sdk_path(LogFunc)

        # path for unity iOS Xcode project and framework on the system
        unity_xcode_project_path, framework_path = get_paths(LogFunc, parser, xcode_sdk_path)

        # edit the Xcode project using mod_pbxproj
        #  add the adSupport framework library
        #  add the iAd framework library
        #  change the compilation flags of the adjust project files to support non-ARC
        edit_unity_xcode_project(LogFunc, unity_xcode_project_path, framework_path)

        # change the Xcode project directly
        #  allow objective-c exceptions
        # rewrite_unity_xcode_project(LogFunc, unity_xcode_project_path)
    sys.exit(0)

def LogInput(writeObject):
    def Log(message, *args):
        messageNLine = (message if message else "None") + "\n"
        writeObject.write(messageNLine.format(*args))
    return Log

def get_paths(Log, parser, xcode_sdk_path):
    args, ignored_args = parser.parse_known_args()
    ios_project_path = args.ios_project_path

    unity_xcode_project_path = ios_project_path + "/Unity-iPhone.xcodeproj/project.pbxproj"
    Log("Unity3d Xcode project path: {0}", unity_xcode_project_path)

    framework_path = xcode_sdk_path + "/System/Library/Frameworks/"
    Log("framework path: {0}", framework_path)

    return unity_xcode_project_path, framework_path

def edit_unity_xcode_project(Log, unity_xcode_project_path, framework_path):
    # load unity iOS pbxproj project file
    unity_XcodeProject = XcodeProject.Load(unity_xcode_project_path)
    
    # add adSupport framework to unity if it's not already there
    unity_XcodeProject.add_file_if_doesnt_exist(framework_path + "AdSupport.framework", tree="SDKROOT", create_build_files=True,weak=True)
    Log("added adSupport framework")

    # add iAd framework to unity if it's not already there
    unity_XcodeProject.add_file_if_doesnt_exist(framework_path + "iAd.framework", tree="SDKROOT", create_build_files=True,weak=True)
    Log("added iAd framework")

    # don't do anything with ARC at the moment
    # regex for adjust sdk files
    # re_adjust_files = re.compile(r"AI.*\.m|.*\+AI.*\.m|Adjust\.m|AdjustUnity\.mm")
    # 
    # 
    # iterate all objects in the unity Xcode iOS project file
    # for key in unity_XcodeProject.get_ids():
    #     obj = unity_XcodeProject.get_obj(key)
    #     
    #     name = obj.get('name')
    #     isa = obj.get('isa')
    #     path = obj.get('path')
    #     fileref = obj.get('fileRef')
    # 
    #     #Log("key: {0}, name: {1}, isa: {2}, path: {3}, fileref: {4}", key, name, isa, path, fileref)
    # 
    #     #check if file reference match any adjust file
    #     adjust_file_match = re_adjust_files.match(name if name else "")
    #     if (adjust_file_match):
    #         #Log("file match, group: {0}", adjust_file_match.group())
    #         # get the build file, from the file reference id
    #         build_files = unity_XcodeProject.get_build_files(key)
    #         for build_file in build_files:
    #             # add the ARC compiler flag to the adjust file if doesn't exist
    #             build_file.add_compiler_flag('-fobjc-arc')
    #             Log("added ARC flag to file {0}", name)

    unity_XcodeProject.add_other_ldflags('-ObjC')

    # save changes
    unity_XcodeProject.saveFormat3_2()

def rewrite_unity_xcode_project(Log, unity_xcode_project_path):
    unity_xcode_lines = []
    # allow objective-c exceptions
    re_objc_excep = re.compile(r"\s*GCC_ENABLE_OBJC_EXCEPTIONS *= *NO.*")
    with open(unity_xcode_project_path) as upf:
        for line in upf:
            if re_objc_excep.match(line):
                #Log("matched line: {0}", re_objc_excep.match(line).group())
                line = line.replace("NO","YES")
                Log("Objective-c exceptions enabled")
            unity_xcode_lines.append(line)
    with open(unity_xcode_project_path, "w+") as upf:
        upf.writelines(unity_xcode_lines)

def get_xcode_sdk_path(Log):
    # outputs all info from xcode
    proc = Popen(["xcodebuild", "-version", "-sdk"], stdout=PIPE, stderr=PIPE)
    out, err = proc.communicate()
    
    if proc.returncode not in [0, 66]:
        Log("Could not retrieve Xcode sdk path. code: {0}, err: {1}", proc.returncode, err)
        return None

    match = re.search("iPhoneOS.*?Path: (?P<sdk_path>.*?)\n", out, re.DOTALL)
    xcode_sdk_path = match.group('sdk_path') if match else None
    Log("Xcode sdk path: {0}", xcode_sdk_path)
    return xcode_sdk_path

if __name__ == "__main__":
    main()
