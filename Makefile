MCS = mcs

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
	Preferences.cs \
	ReverseIterator.cs \
	SaveLoad.cs \
	Schematic.cs \
	Splash.cs

GDKCAIRO = \
	gdk-cairo.cs

RESOURCES = \
	About.glade \
	FatalError.glade \
	LoadError.glade \
	MainWindow.glade \
	PluginAbout.glade \
	PluginError.glade \
	PluginList.glade \
	Preferences.glade \
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
	pixmaps/media-playback-start-22.png \
	pixmaps/media-playback-start.png \
	pixmaps/media-playback-stop-22.png \
	pixmaps/media-playback-stop.png \
	pixmaps/no-base.png \
	pixmaps/plugin-16.png \
	pixmaps/plugin-48.png \
	pixmaps/preferences-desktop-48.png \
	pixmaps/preferences-desktop.png \
	pixmaps/system-log-out.png \
	pixmaps/system-search.png \
	pixmaps/user-trash-full.png \
	pixmaps/zsrr.jpg

RESFILES = $(addprefix resources/,$(RESOURCES))
RESCMD = $(addprefix -resource:,$(RESFILES))

all: eithne.exe
	+make -C Plugins
	+make -C locale

eithne.exe: IPlugin.dll gdk-cairo.dll $(EITHNE) $(RESFILES)
	$(MCS) $(EITHNE) -out:eithne.exe -r:IPlugin -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0 -r:Mono.Cairo -r:gdk-cairo -r:Mono.Posix $(RESCMD) -win32icon:resources/pixmaps/icon.ico -debug -target:winexe

IPlugin.dll: IPlugin/*.cs
	+make -C IPlugin

gdk-cairo.dll: $(GDKCAIRO)
	$(MCS) $(GDKCAIRO) -out:gdk-cairo.dll -target:library -r:Mono.Cairo -pkg:gtk-sharp-2.0 -debug

clean:
	rm -f *.dll eithne.exe *.mdb
	+make -C IPlugin clean
	+make -C Plugins clean
