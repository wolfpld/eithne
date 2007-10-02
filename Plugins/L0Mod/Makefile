MCS = mcs

TARGET = L0Mod.dll

SRC = \
	L0Mod.cs \
	L0ModSetup.cs

RESOURCES = \
	L0Mod.glade

RESFILES = $(addprefix resources/,$(RESOURCES))
RESCMD = $(addprefix -resource:,$(RESFILES))

all: ../$(TARGET)

../$(TARGET): ../../IPlugin.dll $(SRC) $(RESFILES)
	$(MCS) $(SRC) -out:../$(TARGET) -r:IPlugin -r:Mono.Posix -lib:../.. -target:library -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0 $(RESCMD) -debug

clean:
	rm -f ../$(TARGET)
	rm -f ../$(TARGET).mdb
