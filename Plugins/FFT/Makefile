MCS = mcs

TARGET = FFT.dll

SRC = \
	DCT.cs \
	FFT.cs \
	FFTW.cs \
	Setup.cs

RESOURCES = \
	FFT.glade

RESFILES = $(addprefix resources/,$(RESOURCES))
RESCMD = $(addprefix -resource:,$(RESFILES))

all: ../$(TARGET)

../$(TARGET): ../../IPlugin.dll ../../Utility.dll $(SRC) $(RESFILES)
	$(MCS) $(SRC) -out:../$(TARGET) -r:IPlugin -r:Utility -r:Mono.Posix -lib:../.. -target:library -debug $(RESCMD) -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0
	cp -f $(TARGET).config ..

clean:
	rm -f ../$(TARGET) ../$(TARGET).config
	rm -f ../$(TARGET).mdb
