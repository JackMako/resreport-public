import { useState } from "react";
import "./App.css";
import { ClientReportPack } from "./reports/ClientReportPack";
import { DraBuster } from "./reports/dra/DraBuster";

type ReportTab = "client-report-pack" | "dra-report";

function App() {
  const [activeTab, setActiveTab] = useState<ReportTab>("client-report-pack");

  return (
      <main className="app">
        <h1>Residence Report Generator</h1>

        <nav className="tabs">
          <button
              className={activeTab === "client-report-pack" ? "active" : ""}
              onClick={() => setActiveTab("client-report-pack")}
          >
            Client Report Pack
          </button>

          <button
              className={activeTab === "dra-report" ? "active" : ""}
              onClick={() => setActiveTab("dra-report")}
          >
            DRA Report
          </button>
        </nav>

        {activeTab === "client-report-pack" && <ClientReportPack />}
        {activeTab === "dra-report" && <DraBuster />}
      </main>
  );
}

export default App;