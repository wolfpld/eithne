MCS = mcs

TARGET = ClassDB.dll

SRC = \
	ClassDB.cs \
	ClassDBWindow.cs \
	FileList.cs \
	LoadError.cs \
	Modify.cs \
	Preview.cs

RESOURCES = \
	ClassDB.glade \
	LoadError.glade \
	Modify.glade \
	Preview.glade \
	go-next.png

RESFILES = $(addprefix resources/,$(RESOURCES))
RESCMD = $(addprefix -resource:,$(RESFILES))

all: ../$(TARGET)

../$(TARGET): ../../IPlugin.dll ../../Utility.dll $(SRC) $(RESFILES)
	$(MCS) $(SRC) -out:../$(TARGET) -r:IPlugin -r:Utility -r:Mono.Posix -lib:../.. -target:library -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0 $(RESCMD) -debug

clean:
	rm -f ../$(TARGET)
	rm -f ../$(TARGET).mdb
