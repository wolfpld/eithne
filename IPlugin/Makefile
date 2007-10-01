MCS = mcs

TARGET = ../IPlugin.dll

IPLUGIN = \
	Config.cs \
	IPlugin.cs

all: $(TARGET)

$(TARGET): $(IPLUGIN)
	$(MCS) $(IPLUGIN) -out:$(TARGET) -target:library -r:Mono.Posix -unsafe -debug -pkg:gconf-sharp-2.0

clean:
	rm -f $(TARGET) $(TARGET).mdb
