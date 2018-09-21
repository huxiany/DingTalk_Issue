import Reflux from "reflux";

import Actions from "./actions";
import DB from "../../app/db";

class Store extends Reflux.Store {
    constructor() {
        super();
        this.listenables = Actions;
        this.state = {
            error: false,
            errObj: null,
            result: null
        };
    }

    onImportData(params, cb) {
        let t = this;

        let newState = {};

        DB.Test.importData(params)
            .then(r => {
                newState = { error: false, errObj: null, result: r };
            })
            .catch(err => {
                newState = { error: true, errObj: err };
            })
            .then(() => {
                t.setState(newState);
                cb && cb(newState);
            });
    }
}

export default Store;
