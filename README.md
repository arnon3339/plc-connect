<p align="center">
    <h3 align="center">PLC Connect</h3>
  </a>
</p>

---

## 1️⃣ Build application

```bash
cd path/of/application
dotnet build
```

---

## 2️⃣ Run application
```bash
cd ./bin/Debug/net-<version>
./PlcConnect.exe --address <ip address> --port <port> --command <command>
```

ip address is ip address of PLC.
port is port of PLC.
command is hexadecimal string for being sent to PLC.

### Example
```bash
./PlcConnect.exe --address 192.168.1.10 --port 65432 --command 500000FF03FF000018000400010000006400
```

"500000FF03FF000018000400010000006400" is read command for Misubishi PLC.

The test command can be found in `./data.xlsx` file.