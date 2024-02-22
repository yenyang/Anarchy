import classNames from "classnames";
import { ModuleRegistryExtend } from "modding/types";
import { PropsWithChildren, useEffect, useRef } from "react";
import { createPortal } from "react-dom";

export const AnarchyRowComponent : ModuleRegistryExtend = (Component) => {
    return (props) => {
        const { children, ...otherProps} = props || {};

        const parentRef = useRef<HTMLDivElement>();
        const childRef = useRef<HTMLDivElement>();

        useEffect(() => {
            if (parentRef.current) {
                const div = document.createElement('div');
                childRef.current =div;
                parentRef.current?.insertBefore(div, parentRef.current?.firstChild);
            }
        }, [parentRef?.current]);
    
        const InsertedContent = ({ container, children }: PropsWithChildren<{container: any}>) => {
            return !container ? null : createPortal(children, container);
        }

        return (
            <>
                <Component {...otherProps} ref={parentRef} />
                <InsertedContent container={childRef.current}>
                    <div className="item-content_nNz">
                        <span className={'label_RZX'}>Anarchy</span>
                        <button className="content_ZIz">ClickME</button>
                    </div>
                </InsertedContent>
            </>
        );
    };
}
