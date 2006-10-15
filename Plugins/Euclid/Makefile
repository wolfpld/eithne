MCS = mcs

TARGET = Euclid.dll

SRC = \
	Euclid.cs

all: ../$(TARGET)

../$(TARGET): ../../IPlugin.dll ../../Utility.dll $(SRC)
	$(MCS) $(SRC) -out:../$(TARGET) -r:IPlugin -r:Utility -r:Mono.Posix -lib:../.. -target:library -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0

clean:
	rm -f ../$(TARGET)
