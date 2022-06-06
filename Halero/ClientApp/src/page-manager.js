import { BrowserRouter, Routes, Route } from "react-router-dom";
import React from 'react';
import ReactDOM from 'react-dom';
import cookies from "js-cookies";
import 'react-toastify/dist/ReactToastify.css';

import { AuthenticationPage } from "./authentication/authentication-layout";
import { SnakeLayout } from "./interaction/layout";
import { UserProfile } from "./interaction/profile";
import { SoloPlayment } from "./interaction/soloplayment";
import { MultiplayerPlayment } from "./interaction/multiplayment";
import { MaintenancePage } from "./interaction/maintenance-content";

export class Pager extends React.Component{
    constructor(props){
        super(props);
        
        this.state = {
            accessToken: cookies.getItem("accessToken"),
            refreshToken: cookies.getItem("refreshToken"),
            loginining: null
        };

        // this.verifyToken()
        // setInterval( () => this.verifyToken(), 1000 * 60);
    }

    

    render(){ 
        const preventDefault = (event) => event.preventDefault();

        if(this.state.accessToken == null || this.state.refreshToken == null){
            if( document.location.pathname != "/" ){
                document.location.pathname = "/";
            }
            return (<> <AuthenticationPage /> </>);
        }

        return (
        <BrowserRouter>

            <Routes>
                <Route path="/"                 element = { <SnakeLayout> <SoloPlayment />          </SnakeLayout> } />
                <Route path="/profile"          element = { <SnakeLayout> <UserProfile />           </SnakeLayout> } />
                <Route path="/solo"             element = { <SnakeLayout> <SoloPlayment />          </SnakeLayout> } />
                <Route path="/online/play"      element = { <SnakeLayout> <MultiplayerPlayment />   </SnakeLayout> } />
                <Route path="/online/active"    element = { <SnakeLayout> <MaintenancePage />       </SnakeLayout> } />
            </Routes>

        </BrowserRouter>
        );
    }
}