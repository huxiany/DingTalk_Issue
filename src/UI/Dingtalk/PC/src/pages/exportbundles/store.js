import Reflux from "reflux";
import Actions from "./actions";
import DB from "../../app/db";

class Store extends Reflux.Store {
    constructor() {
        super();
        this.listenables = Actions;
        this.state = {};
    }
}

export default Store;
