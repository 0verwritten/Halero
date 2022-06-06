import React from "react";
import image from "./imgs/maintenance.webp";

export class MaintenancePage extends React.Component{
    render(){
        require('./css/maintenance.css');
        return(
            <>
                <center className="centrimo">
                    <img src={image} alt="non base" />
                    <h3>
                        This page is under maintenance Г:
                    </h3>
                </center>
            </>
        );
    }
}