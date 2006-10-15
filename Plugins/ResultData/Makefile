MCS = mcs

TARGET = ResultData.dll

SRC = \
	ResultData.cs \
	ResultDataWindow.cs

RESOURCES = \
	ResultData.glade \
	testbase.png

RESFILES = $(addprefix resources/,$(RESOURCES))
RESCMD = $(addprefix -resource:,$(RESFILES))

all: ../$(TARGET)

../$(TARGET): ../../IPlugin.dll ../../Utility.dll $(SRC) $(RESFILES)
	$(MCS) $(SRC) -out:../$(TARGET) -r:IPlugin -r:Utility -r:Mono.Posix -lib:../.. -target:library -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0 $(RESCMD)

clean:
	rm -f ../$(TARGET)
