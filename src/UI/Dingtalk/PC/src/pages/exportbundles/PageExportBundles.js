import settings from "../../components/Settings";
import React from "react";
import { message, Button } from "antd";

class PageExportBundles extends React.Component {
    handlePrintClicked() {
        let t = this;
        let s = t.state;

        let webApiUrl = settings.webApiUrl;

        let printAPI = "Test/Print";
        message.info("正在导出码单以供下载并打印...");
        window.location = webApiUrl + printAPI;
    }

    render() {
        let t = this;

        return (
            <div>
                <div>
                    <Button
                        type="primary"
                        size="large"
                        style={{ marginLeft: "10px", padding: "4px 4px" }}
                        onClick={t.handlePrintClicked.bind(t)}
                    >
                        导出数据
                    </Button>
                </div>
            </div>
        );
    }
}

export default PageExportBundles;
