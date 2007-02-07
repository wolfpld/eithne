MCS = gmcs

TARGET = SimpleResultView.dll

SRC = \
	ResultSetup.cs \
	ResultView.cs \
	ResultViewWindow.cs

RESOURCES = \
	ResultSetup.glade \
	ResultView.glade

RESFILES = $(addprefix resources/,$(RESOURCES))
RESCMD = $(addprefix -resource:,$(RESFILES))

all: ../$(TARGET)

../$(TARGET): ../../IPlugin.dll ../../Utility.dll $(SRC) $(RESFILES)
	$(MCS) $(SRC) -out:../$(TARGET) -r:IPlugin -r:Utility -r:Mono.Posix -lib:../.. -target:library -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0 $(RESCMD) -debug

clean:
	rm -f ../$(TARGET)
	rm -f ../$(TARGET).mdb
