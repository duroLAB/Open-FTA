# Open FTA (Fault Tree Analysis)

**Open FTA** is a simple application designed to perform fault tree analysis for safety assessment in the chemical industry. The application serves as a **teaching tool** for educating chemical engineers and as a **free (open-source) alternative** to commercial systems.  

Being open-source allows for further development of the application, verification and auditing of calculations, and adaptation to specific user needs. It can be used in **research projects** and in the **educational process**, for example in courses such as safety engineering.  

The foundation of the application is based on the diploma thesis of M. Doller: [“Design of a Software System for Fault Tree Analysis”](https://kis.cvt.stuba.sk/arl-stu/sk/detail-stu_us_cat-stuzp105108-Navrh-softveroveho-systemu-na-analyzu-stromu-poruch/).  

---

## Key Features

- Calculation of the frequency or probability of the top event (gate by gate and from minimal cut sets).  
- Identification of **Minimal Cut Sets (MCS)** and drawing of the **minimal cut set tree**.  
- **Importance measures** of basic events (Birnbaum, criticality, RAW, RRW, Fussell–Vesely).  
- Database browser for easier assignment of basic event frequencies.  
- Automatic tree layout, including the compact tidy-tree algorithm (ALGO III).  
- Export of analysis results as detailed reports (top event frequency, list of basic events with references, MCS analysis, importance measures).  
- Export of the fault tree and of the minimal cut set tree as **bitmap** or **vector graphics**.  

## Installation

1. Go to the [Releases](https://github.com/duroLAB/Open-FTA/releases) page on GitHub.  
2. Download the latest ZIP archive.  
3. Extract the contents of the ZIP to a folder of your choice.  
4. Open the folder and run the executable (`Open FTA.exe`).  

## License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.  

You are free to **use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies** of this software, as long as the copyright notice and license text are included.  

This software is provided "as is", without warranty of any kind.

## Changelog

For a complete list of changes and release notes, see [Changelog.md](Changelog.md).