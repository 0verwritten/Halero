import { getCookie, removeCookie, setCookie } from 'typescript-cookie';
import { AuthenticationForm } from "../models/authentication-models";

export function verifyToken(){
  if( getCookie("accessToken") !== undefined && getCookie("refreshToken") !== undefined ){
      return fetch("/api/authentication/checkToken", { method: "POST" })
          .then(e => {
                if(e.status === 200)
                    return e.text()
                removeCookie("accessToken", { path: "/" });
                removeCookie("refreshToken", { path: "/" });
          }).then( data => {
              console.log(data);
              if(data === "false"){
                  if( getCookie("accessToken") !== undefined && getCookie("refreshToken") !== undefined ){
                        return refreshToken();
                  }
                  removeCookie("accessToken", { path: "/" });
                  removeCookie("refreshToken", { path: "/" });
                  return false;
              }else{
                  return true;
              }
          } );
  }
  return false;
}

function refreshToken(){
  fetch("/api/authentication/update", { method: "POST" }).then( e => e.json() ).then( (responce: AuthenticationForm) => {
      if(responce.token !== null){
          setCookie("refreshToken", responce.token.refreshToken);
          setCookie("accessToken", responce.token.accessToken);
      }else{
          removeCookie("accessToken", { path: "/" });
          removeCookie("refreshToken", { path: "/" });
      }
      window.location.reload();
      return responce.token !== null;
  }).finally();
}
