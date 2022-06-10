import React from 'react';
import { BrowserRouter, Routes, Route } from "react-router-dom";

import { getCookie } from 'typescript-cookie';
import 'react-toastify/dist/ReactToastify.css';

import { AuthenticationPage } from "./authentication/authentication-layout";
import { SnakeLayout } from "./interaction/layout";
import { UserProfile } from "./interaction/profile";
import { SoloPlayment } from "./interaction/soloplayment";
import { MultiplayerPlayment } from "./interaction/multiplayment";
import { MaintenancePage } from "./interaction/maintenance-content";

type PagerStates = {
    accessToken: string | undefined;
    refreshToken: string | undefined;
}

export class Pager extends React.Component<{}, PagerStates>{

    constructor(props: {}){
        super(props);
        
        this.state = {
            accessToken: getCookie("accessToken"),
            refreshToken: getCookie("refreshToken")
        };

        // this.verifyToken()
        // setInterval( () => this.verifyToken(), 1000 * 60);
    }

    

    render(){ 

        if(this.state.accessToken === undefined || this.state.refreshToken === undefined){
            if( document.location.pathname !== "/" ){
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