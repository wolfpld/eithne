MCS = mcs

TARGET = FFT.dll

SRC = \
	DCT.cs \
	FFT.cs \
	FFTW.cs

all: ../$(TARGET)

../$(TARGET): ../../IPlugin.dll ../../Utility.dll $(SRC)
	$(MCS) $(SRC) -out:../$(TARGET) -r:IPlugin -r:Utility -r:Mono.Posix -lib:../.. -target:library -debug
	cp -f $(TARGET).config ..

clean:
	rm -f ../$(TARGET) ../$(TARGET).config
	rm -f ../$(TARGET).mdb
