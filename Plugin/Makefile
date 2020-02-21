IOS_REPO_NAME := AdColony-iOS-SDK
ANDROID_REPO_NAME := AdColony-Android-SDK
UNITY_PLUGIN_DIR := $(shell pwd)
SDK_DIR := $(UNITY_PLUGIN_DIR)/src/sdks
IOS_DIR := $(SDK_DIR)/$(IOS_REPO_NAME)
ANDROID_DIR := $(SDK_DIR)/$(ANDROID_REPO_NAME)
ANDROID_FINAL_JAR := $(ANDROID_DIR)/Library/adcolony.jar
ANDROID_FINAL_WRAPPER_DIR := $(UNITY_PLUGIN_DIR)/src/android
ANDROID_FINAL_WRAPPER_JAR := $(UNITY_PLUGIN_DIR)/src/android/Library/build/outputs/jar/adcolony-wrapper.jar
UNITY_PACKAGE_FULL_PATH := $(UNITY_PLUGIN_DIR)/AdColony.unitypackage
UNITY_TEMP_PROJECT_DIR := $(UNITY_PLUGIN_DIR)/tempunitypackage
UNITY_BINARY := /Applications/Unity/Unity.app/Contents/MacOS/Unity
UNITY_LOG_FILE := $(UNITY_PACKAGE_FULL_PATH).build.log
PLUGIN_PACKAGE_FULL_PATH := $(UNITY_PLUGIN_DIR)/AdColony.unitypackage

.PHONY: clean

all: unity

clean: clean_android clean_ios clean_unity

# ---- iOS ----

ios:
	@if [ ! -x "$(IOS_DIR)/AdColony.framework" ]; then \
		echo "\nERROR: AdColony iOS SDK not found\n"; \
		echo "Make sure you have a copy of the native repository in the Plugin/src/sdks directory, run: \n  cd "$(SDK_DIR)"\n  git clone https://github.com/AdColony/$(IOS_REPO_NAME).git\n"; \
		exit 1; \
	fi;

	@# Move to plugin folder
	rm -rf "$(UNITY_PLUGIN_DIR)/Assets/Plugins/iOS/AdColony/AdColony.framework"
	cp -r "$(IOS_DIR)/AdColony.framework" "$(UNITY_PLUGIN_DIR)/Assets/Plugins/iOS/AdColony"
	rm -rf "$(UNITY_PLUGIN_DIR)/Assets/Plugins/iOS/AdColony/AdColony.framework/Versions"

clean_ios:
	rm -rf "$(UNITY_PLUGIN_DIR)/Assets/Plugins/iOS/AdColony/AdColony.framework"

# ---- Android ----

android_jar:
	@if [ ! -f "$(ANDROID_FINAL_JAR)" ]; then \
		echo "\nERROR: AdColony Android SDK not found\n"; \
		echo "Make sure you have a copy of the native repository in the Plugin/src/sdks directory, run: \n  cd "$(SDK_DIR)"\n  git clone https://github.com/AdColony/$(ANDROID_REPO_NAME).git\n"; \
		exit 1; \
	fi;

clean_android_jar:
	rm -f "$(UNITY_PLUGIN_DIR)/Assets/Plugins/Android/AdColony/libs/AdColony.jar"
	find "$(UNITY_PLUGIN_DIR)/Assets/Plugins/Android/libs" -name "libadcolony.so" -exec rm "{}" +
	find "$(UNITY_PLUGIN_DIR)/Assets/Plugins/Android/libs" -name "libjs.so" -exec rm "{}" +

android_wrapper: android_jar
	cp "$(ANDROID_FINAL_JAR)" "$(ANDROID_FINAL_WRAPPER_DIR)/Library/libs/AdColony.jar"
	cp -r "$(ANDROID_DIR)/Library"/* "$(ANDROID_FINAL_WRAPPER_DIR)/Library/libs"
	cd "$(ANDROID_FINAL_WRAPPER_DIR)" && ./gradlew clean && ./gradlew makeJar

clean_android_wrapper:
	rm -rf "$(ANDROID_FINAL_WRAPPER_DIR)/build"
	rm -rf "$(ANDROID_FINAL_WRAPPER_DIR)/Library/build"
	rm -f "$(ANDROID_FINAL_WRAPPER_DIR)/Library/libs/AdColony.jar"
	rm -f "$(UNITY_PLUGIN_DIR)/Assets/Plugins/Android/AdColony/libs/AdColonyUnityWrapper.jar"
	find "$(ANDROID_FINAL_WRAPPER_DIR)/Library/libs" -name "libadcolony.so" -exec rm "{}" +
	find "$(ANDROID_FINAL_WRAPPER_DIR)/Library/libs" -name "libjs.so" -exec rm "{}" +

android: android_jar android_wrapper
	@# Move to plugin folder
	mkdir -p "$(UNITY_PLUGIN_DIR)/Assets/Plugins/Android/AdColony/libs"
	cp "$(ANDROID_FINAL_JAR)" "$(UNITY_PLUGIN_DIR)/Assets/Plugins/Android/AdColony/libs/AdColony.jar"
	cp "$(ANDROID_FINAL_WRAPPER_JAR)" "$(UNITY_PLUGIN_DIR)/Assets/Plugins/Android/AdColony/libs/AdColonyUnityWrapper.jar"

clean_android: clean_android_jar clean_android_wrapper

# ---- Unity ----

unity_package:
	@echo "Logfile: $(UNITY_LOG_FILE)"
	@echo "Building Unity package: $(UNITY_PACKAGE_FULL_PATH)"
	@rm -rf "$(UNITY_PACKAGE_FULL_PATH)"
	@rm -rf "$(UNITY_TEMP_PROJECT_DIR)"
	@mkdir "$(UNITY_TEMP_PROJECT_DIR)"
	@cp -r "$(UNITY_PLUGIN_DIR)/Assets" "$(UNITY_TEMP_PROJECT_DIR)"

	@if [ ! -x "$(UNITY_BINARY)" ]; then \
		echo "\nERROR: $(UNITY_BINARY) not found\nYou might have to change the path of the Unity binary in the makefile, or move the Unity application to the default location."; exit 1; \
	fi;

	@if [ -x "$(UNITY_BINARY)" ]; then \
		"$(UNITY_BINARY)" \
		-batchmode \
		-quit \
		-force-free \
		-logFile "$(UNITY_LOG_FILE)" \
		-projectPath "$(UNITY_TEMP_PROJECT_DIR)" \
		-exportPackage "Assets" "$(UNITY_PACKAGE_FULL_PATH)" \
		|| echo "ERROR: Error building unity package, see log: $(UNITY_LOG_FILE)"; \
	else \
		@echo "ERROR: Unity package not created, looked for Unity at $(UNITY_BINARY)."; \
	fi;

	@rm -rf "$(UNITY_TEMP_PROJECT_DIR)"

unity: android ios unity_package

clean_unity:
	rm -rf "$(UNITY_TEMP_PROJECT_DIR)"
	rm -f "$(UNITY_PACKAGE_FULL_PATH)"
	rm -f "$(UNITY_LOG_FILE)"
