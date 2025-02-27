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
./PlcConnect.exe --address <ip address> --port <port> [options] 
```

**ip address** is ip address of PLC (*required!*). \
**port** is port of PLC (*required!*).

### 1.1 Command mode 
```bash
./PlcConnect.exe --address <ip address> --port <port> --command <command value> --binary
```

**command** is hexadecimal string for being sent to PLC (*required!*). \
The **command** value is ASCII string by default and it will be converted to bytes before sending to PLC. The **binary** option is provided to use raw binay from **command** value directly send to PLC.

The test command can be found in `./data.xlsx` file.

#### Example
```bash
./PlcConnect.exe --address 196.0.0.1 --port 65432 --command 500000FF03FF000018000400010000006400
```

### 1.2 Write mode
```bash
./PlcConnect.exe --address <ip address> --port <port> --head <head address> --value <write value>
```

**head** is an address of head device (*required!*). \
**value** is a value sent to PLC (*required!*). 

#### Example
```bash
./PlcConnect.exe --address 196.0.0.1 --port 65432 --head M1000 --value 1
```

## 3️⃣ Configuration
The configuration is usually applied to **read** and **write** mode.
```toml
[default]
subheader = "5000"
network_no = "00"
pc_station_no = "FF"
io_unit_address = "03FF"
request_length = "00001800"
cpu_monitoring_timer = "0414"

[default.read]
command = ""
subcommand = ""

[default.write]
command = "0200"
subcommand = "0101"
```

| Field                 | Size (chars) | Example Bytes | Meaning                                                    |
|-----------------------|--------------|---------------|----------------------------------------------------------|
| **Subheader**        | 4            | `5000`        | 3E frame in ASCII ("50 00" as ASCII hex).                 |
| **Network No.**      | 2            | `00`          | Usually `00` if local network.                            |
| **PC Station No.**    | 2            | `FF`          | Often `FF` for "no particular station".                   |
| **I/O Unit Addr.**    | 4            | `03FF`        | (Varies) Used to indicate target module I/O address in some configurations. |
| **Request Length**    | 4            | `0000`/`1800` | The total "data length" (in bytes) of the command portion that follows. |
| **CPU Monitoring Timer** | 4        | `0414`        | The watch-dog / time-out setting (in hex). Often you see `0010` etc. |
| **Command**          | 4            | `0200` or `1402` | Which MC command (e.g. read, write).                       |
| **Subcommand**       | 4            | `0101` or similar | Command detail.                                           |
| **Data**             | variable     | `M*00100100`   | The device specification and the actual data to be written (or read) in ASCII. |

The **Data** in the configuration table is obtained from parsing **head** and **value** from command line arguments.