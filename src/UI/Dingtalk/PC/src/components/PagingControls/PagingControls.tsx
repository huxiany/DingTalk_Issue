import "./PagingControls.styl";

import { Button } from "antd";
import PropTypes from "prop-types";
import React from "react";

declare interface IPagingControlsProps extends React.Props<PagingControls> {
    autocycle: boolean;
    allowPageSwitching: boolean;
    onPreviousReportClick: () => void;
    onPreviousPageClick: () => void;
    onNextPageClick: () => void;
    onNextReportClick: () => void;
    onPauseClick: () => void;
    onContinueClick: () => void;
}

class PagingControls extends React.Component<IPagingControlsProps> {
    public static defaultProps: IPagingControlsProps;

    public render() {
        const t = this;

        const { autocycle, allowPageSwitching } = t.props;

        let pagingControlsWidth = 168;
        if (allowPageSwitching) {
            pagingControlsWidth = 280;
        }

        return (
            <div className="controlArea">
                <div
                    className="buttonField"
                    style={{ width: pagingControlsWidth }}
                >
                    <Button
                        icon="verticle-right"
                        className="iconStyle"
                        onClick={t.props.onPreviousReportClick.bind(t)}
                    />
                    {allowPageSwitching ? (
                        <Button
                            icon="left"
                            className="iconStyle"
                            onClick={t.props.onPreviousPageClick.bind(t)}
                        />
                    ) : null}
                    {autocycle ? (
                        <Button
                            icon="pause-circle-o"
                            className="iconStyle"
                            onClick={t.props.onPauseClick.bind(t)}
                        />
                    ) : (
                        <Button
                            icon="play-circle-o"
                            className="iconStyle"
                            onClick={t.props.onContinueClick.bind(t)}
                        />
                    )}
                    {allowPageSwitching ? (
                        <Button
                            icon="right"
                            className="iconStyle"
                            onClick={t.props.onNextPageClick.bind(t)}
                        />
                    ) : null}
                    <Button
                        icon="verticle-left"
                        className="iconStyle"
                        onClick={t.props.onNextReportClick.bind(t)}
                    />
                </div>
            </div>
        );
    }
}

PagingControls.defaultProps = {
    autocycle: true,
    allowPageSwitching: false,
    onPreviousReportClick: () => {},
    onPreviousPageClick: () => {},
    onNextPageClick: () => {},
    onNextReportClick: () => {},
    onPauseClick: () => {},
    onContinueClick: () => {}
};

export default PagingControls;
