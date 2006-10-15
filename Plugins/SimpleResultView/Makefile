MCS = mcs

TARGET = SimpleResultView.dll

SRC = \
	ResultView.cs \
	ResultViewWindow.cs

RESOURCES = \
	ResultView.glade

RESFILES = $(addprefix resources/,$(RESOURCES))
RESCMD = $(addprefix -resource:,$(RESFILES))

all: ../$(TARGET)

../$(TARGET): ../../IPlugin.dll ../../Utility.dll $(SRC) $(RESFILES)
	$(MCS) $(SRC) -out:../$(TARGET) -r:IPlugin -r:Utility -r:Mono.Posix -lib:../.. -target:library -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0 $(RESCMD)

clean:
	rm -f ../$(TARGET)
