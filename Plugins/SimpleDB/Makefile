MCS = mcs

TARGET = SimpleDB.dll

SRC = \
	FileList.cs \
	LoadError.cs \
	Preview.cs \
	SimpleDB.cs \
	SimpleDBWindow.cs

RESOURCES = \
	LoadError.glade \
	Preview.glade \
	SimpleDB.glade

RESFILES = $(addprefix resources/,$(RESOURCES))
RESCMD = $(addprefix -resource:,$(RESFILES))

all: ../$(TARGET)

../$(TARGET): ../../IPlugin.dll $(SRC) $(RESFILES)
	$(MCS) $(SRC) -out:../$(TARGET) -r:IPlugin -r:Mono.Posix -lib:../.. -target:library -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0 $(RESCMD) -debug -r:System.Drawing

clean:
	rm -f ../$(TARGET)
	rm -f ../$(TARGET).mdb
