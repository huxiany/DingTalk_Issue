import Reflux from "reflux";
import React from "react";
import { Input, Button, message, Spin, Row, Col } from "antd";
import Actions from "./actions";
import Store from "./store";
import dtClientSVC from "../../components/DingtalkClientSVC";

class PageImportNewPolishingData extends Reflux.Component {
    constructor(props) {
        super(props);
        this.state = {
            fileName: null
        };
        this.store = Store;
        this.dzConfig = {
            iconFiletypes: [".xlsx"],
            showFiletypeIcon: true,
            postUrl: "no-url"
        };
        this.djsConfig = {
            autoProcessQueue: false,
            acceptedFiles: ".xlsx",
            maxFiles: 1,
            maxFilesize: 2
        };
        this.eventHandlers = {
            init: this.initDZ.bind(this),
            maxfilesexceeded: this.showFileLimitWarning
        };
        this.dzObj = null;
    }

    showFileLimitWarning(file) {
        message.warning("每次只能上传一个文件");
        this.removeFile(file);
    }

    initDZ(dzObj) {
        this.dzObj = dzObj;
    }

    importFile() {
        let t = this;

        let fileDOM = this.refs.file;

        if (fileDOM.files.length == 0) {
            message.error("请选择一个.xlsx文件进行导入");
            return;
        }

        t.stageUploading();

        let file = fileDOM.files[0];

        let reader = new FileReader();

        reader.onload = async evt => {
            let fileContent = evt.target.result;
            let fileName = file.name;
            if (fileContent.length > 0 && fileContent.includes("base64,")) {
                fileContent = fileContent.split("base64,")[1];
            }
            if (fileContent.length > 0) {
                try {
                    let tempAuthCode = await dtClientSVC.getDDAuthCodeForMessageAsync();
                    let params = {
                        fileName: fileName,
                        fileContent: fileContent,
                        authCode: tempAuthCode
                    };
                    Actions.importData(params, rs => {
                        t.finishUploading();
                        if (rs.error) {
                            let err = rs.errObj;
                            message.error(
                                "数据导入出错，请重试。详细信息：" +
                                    JSON.stringify(err)
                            );
                        } else {
                            message.success("成功导入数据");
                        }
                    });
                } catch (err) {
                    t.finishUploading();
                    message.error(
                        "遇到不可恢复错误，请关闭工作台重新进入再试一次。详细信息：" +
                            JSON.stringify(err)
                    );
                }
            }
        };
        reader.readAsDataURL(file);
    }

    selectFile() {
        this.refs.file.click();
    }

    fileChanged(evt) {
        this.setState({ fileName: evt.target.files[0].name });
    }

    stageUploading() {
        this.setState({ uploading: true });
    }

    finishUploading() {
        this.setState({ uploading: false });
    }

    render() {
        let t = this;
        let s = t.state;

        return (
            <div>
                <p>导入数据</p>
                <Row style={{ marginTop: "20px" }}>
                    <Col span={8}>
                        <Input
                            size="large"
                            disabled={true}
                            value={s.fileName}
                        />
                        <input
                            type="file"
                            ref="file"
                            accept=".xlsx"
                            style={{ display: "none" }}
                            onChange={t.fileChanged.bind(t)}
                        />
                    </Col>
                    <Col span={16}>
                        <Button
                            ref="fileSelectionBtn"
                            type="primary"
                            size="large"
                            onClick={t.selectFile.bind(t)}
                            style={{ marginLeft: "10px" }}
                            disabled={s.uploading}
                        >
                            选择文件
                        </Button>
                        <Button
                            onClick={t.importFile.bind(t)}
                            ref="uploadBtn"
                            type="primary"
                            size="large"
                            style={{ marginLeft: "10px" }}
                            disabled={s.uploading}
                        >
                            上传并导入码单
                        </Button>
                    </Col>
                </Row>

                {s.uploading ? (
                    <div className="importingSpinArea" ref="spinArea">
                        <p>正在上传文件并导入数据，请等待...</p>
                        <Spin />
                    </div>
                ) : null}
            </div>
        );
    }
}

export default PageImportNewPolishingData;
