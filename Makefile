MCS = mcs

IPLUGIN = \
	IPlugin.cs

EITHNE = \
	About.cs \
	Block.cs \
	Connector.cs \
	Engine.cs \
	FatalError.cs \
	LoadError.cs \
	MainWindow.cs \
	PluginAbout.cs \
	PluginDB.cs \
	PluginError.cs \
	PluginList.cs \
	PluginToolbox.cs \
	ReverseIterator.cs \
	SaveLoad.cs \
	Schematic.cs \
	Splash.cs

GDKCAIRO = \
	gdk-cairo.cs

UTILITY = \
	DialogMessage.cs \
	DialogQuestion.cs \
	Utility.cs

UTILITY_RESOURCES = \
	DialogMessage.glade \
	DialogQuestion.glade

RESOURCES = \
	About.glade \
	FatalError.glade \
	LoadError.glade \
	MainWindow.glade \
	PluginAbout.glade \
	PluginError.glade \
	PluginList.glade \
	Splash.glade \
	pixmaps/dialog-error-16.png \
	pixmaps/dialog-error.png \
	pixmaps/dialog-information-16.png \
	pixmaps/dialog-information.png \
	pixmaps/dialog-warning-16.png \
	pixmaps/dialog-warning.png \
	pixmaps/document-new-22.png \
	pixmaps/document-new.png \
	pixmaps/document-open-22.png \
	pixmaps/document-open.png \
	pixmaps/document-save-22.png \
	pixmaps/document-save-as.png \
	pixmaps/document-save.png \
	pixmaps/edit-cut.png \
	pixmaps/edit-delete.png \
	pixmaps/help-browser-48.png \
	pixmaps/help-browser.png \
	pixmaps/icon-16.png \
	pixmaps/icon-48.png \
	pixmaps/image-base-22.png \
	pixmaps/image-base.png \
	pixmaps/image-test-22.png \
	pixmaps/image-test.png \
	pixmaps/list-add.png \
	pixmaps/list-remove.png \
	pixmaps/media-playback-start.png \
	pixmaps/media-playback-start-22.png \
	pixmaps/plugin-16.png \
	pixmaps/plugin-48.png \
	pixmaps/preferences-desktop.png \
	pixmaps/system-log-out.png \
	pixmaps/system-search.png \
	pixmaps/user-trash-full.png \
	pixmaps/zsrr.jpg

RESFILES = $(addprefix resources/,$(RESOURCES))
RESCMD = $(addprefix -resource:,$(RESFILES))

URESFILES = $(addprefix resources/,$(UTILITY_RESOURCES))
URESCMD = $(addprefix -resource:,$(URESFILES))

all: eithne.exe
	make -C Plugins
	make -C locale

eithne.exe: IPlugin.dll gdk-cairo.dll Utility.dll $(EITHNE) $(RESFILES)
	$(MCS) $(EITHNE) -out:eithne.exe -r:IPlugin -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0 -r:Mono.Cairo -r:gdk-cairo -r:Mono.Posix -r:Utility $(RESCMD) -win32icon:resources/pixmaps/icon-48.ico -debug -target:winexe

IPlugin.dll: $(IPLUGIN)
	$(MCS) $(IPLUGIN) -target:library -r:Mono.Cairo

gdk-cairo.dll: $(GDKCAIRO)
	$(MCS) $(GDKCAIRO) -target:library -r:Mono.Cairo -pkg:gtk-sharp-2.0

Utility.dll: $(UTILITY) $(URESFILES)
	$(MCS) $(UTILITY) -target:library -out:Utility.dll -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0 -r:IPlugin -r:Mono.Posix -unsafe $(URESCMD)

clean:
	rm -f *.dll eithne.exe eithne.exe.mdb
	make -C Plugins clean
