import cookies from 'js-cookies';

export function verifyToken(){
  if( cookies.hasItem("accessToken") && cookies.hasItem("refreshToken") ){
      return fetch("/api/authentication/checkToken", { method: "POST" })
          .then(e => e.text()).then( data => {
              console.log(data);
              if(data == "false"){
                  if( cookies.hasItem("accessToken") && cookies.hasItem("refreshToken") ){
                        return refreshToken();
                  }
                  cookies.removeItem("accessToken", "/");
                  cookies.removeItem("refreshToken", "/");
                  return false;
              }else{
                  return true;
              }
          } );
  }
  return false;
}

function refreshToken(){
  fetch("/api/authentication/update", { method: "POST" }).then( e => e.json() ).then( responce => {
      if(responce.token != null){
          cookies.setItem("refreshToken", responce.token.refreshToken);
          cookies.setItem("accessToken", responce.token.accessToken);
      }else{
          cookies.removeItem("accessToken", "/");
          cookies.removeItem("refreshToken", "/");
      }
      window.location.reload();
      return responce.token != null;
  }).finally();
}
