import { ModuleRegistryExtend } from "modding/types";

export const AnarchyRowComponent : ModuleRegistryExtend = (Component) => {
    
    return (props) => {
        const { children, ...otherProps } = props || {};
        console.log("Hello Anarchy!");
        return (
            <Component {...otherProps}>
                 <div className = "item_bZY" id = "YYA-anarchy-item"> 
                    <div className="item-content_nNz">
                    <div className="label_RZX">Anarchy2</div>
                        <div className="content_ZIz">
                            <button id="YYA-Anarchy-Button" className="button_KVN">
                                <img id="YYA-Anarchy-Image" className="icon_Ysc" src="coui://ui-mods/images/StandardAnarchy.svg"></img>
                            </button>
                        </div>
                    </div>
                </div>
                {children}
            </Component>
        );
    };
}