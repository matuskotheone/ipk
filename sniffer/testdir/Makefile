# Define the default target
all: build

# Define the build target
build:
	dotnet build -o .

# Define the clean target
clean:
	dotnet clean
	rm -rf ipk-sniffer/bin
	rm -rf ipk-sniffer/obj
	rm ipk-sniffer.deps.json
	rm ipk-sniffer.dll
	rm ipk-sniffer.pdb
	rm ipk-sniffer.runtimeconfig.json
	rm SharpPcap.dll 
	rm PacketDotNet.dll
	rm ipk-sniffer
