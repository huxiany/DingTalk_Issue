import "./PageExportBundles.styl";
import settings from "../../components/Settings";

import React from "react";
import Reflux from "reflux";

import Actions from "./actions";
import Store from "./store";
import {
    Table,
    message,
    Icon,
    Input,
    Button,
    Tag,
    Tooltip,
    Pagination
} from "antd";

class PageExportBundles extends Reflux.Component {
    constructor(props) {
        super(props);
        this.state = {
            selectedBundleIds: [], // 存放已选择打印的扎的Id
            selectedBundles: [] // 存放已选择打印的扎的信息
        };
        this.store = Store;
    }

    componentDidMount() {}

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
        let s = t.state;

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
