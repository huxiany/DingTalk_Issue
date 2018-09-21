import React from "react";
import "./PageHome.styl";

class PageHome extends React.Component {
    constructor(props) {
        super(props);
        this.state = {};
    }

    public componentDidMount() {}

    public render() {
        return (
            <div>
                <p>欢迎使用石材销售订单和生产跟踪系统</p>
                <p>请从左边菜单选择所需功能</p>
            </div>
        );
    }
}

export default PageHome;
