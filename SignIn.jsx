import { Fragment, React, useState } from "react";
import { Link } from "react-router-dom";
import { Col, Row, Card, Button, Image } from "react-bootstrap";
import FloatingLabel from "react-bootstrap/FloatingLabel";
import { Formik, ErrorMessage, Field, Form } from "formik"; //useField
import logoround from "../../../../assets/images/brand/wePairLogos/logoround.png";
import { signInValSchema } from "../../../../schema/signInValidation";
import { login } from "services/userService";
import logger from "debug";
import "./signup.css";
import Swal from "sweetalert2";
import toastr from "toastr";
import { ReactComponent as Slasheyeicon } from "../../../../assets/images/icons/slasheyeicon.svg";
import { ReactComponent as Eyeicon } from "../../../../assets/images/icons/eyeicon.svg";

const _logger = logger.extend("users:signup");
const SignIn = () => {
  const [formData] = useState({ Email: "", Password: "" });

  const [showPassword, setShowPassword] = useState(false);

  const togglePasswordVisibility = () => {
    setShowPassword((prevState) => !prevState);
  };

  function submitForm(values) {
    _logger(values);
    login(values).then(onLoginSuccess).catch(onLoginError);
  }
  const onLoginSuccess = (response) => {
    toastr.success("Login Successful!");
    _logger(response);
  };

  const onLoginError = (error) => {
    const errorMessage = error?.response?.data?.errors?.[0];
    _logger("error", error);
    if (errorMessage.includes("Email")) {
      Swal.fire({
        icon: "error",
        title: "Error",
        text: "An account with that email does not exist.",
      });
    } else {
      Swal.fire({
        icon: "error",
        title: "Oops...",
        text: "Please check your password and try again.",
      });
    }
  };
  return (
    <Fragment>
      <Formik
        enableReinitialize
        initialValues={formData}
        validationSchema={signInValSchema}
        onSubmit={submitForm}>
        <Row className="align-items-center justify-content-center g-0 min-vh-100">
          <Col lg={5} md={5} className="py-8 py-xl-0">
            <Card className="card-shadow">
              <Card.Body className="p-6">
                <div className="mb-4">
                  <Link to="/" className="cent">
                    <Image src={logoround} className="mb-4  logo-styling" alt="" />
                  </Link>
                  <h1 className="mb-1 fw-bold cent">Sign In</h1>
                  <div className="cent">
                    <span>
                      Don&apos;t have an account?{" "}
                      <Link to="/signup" className="ms-1 ">
                        Sign Up
                      </Link>
                    </span>
                  </div>
                </div>

                <Form>
                  <div className="form-group m-3">
                    <FloatingLabel controlId="floatingInputGrid" label="Email Address">
                      <Field
                        type="email"
                        name="Email"
                        className="form-control"
                        placeholder="name@example.com"
                      />
                    </FloatingLabel>
                    <ErrorMessage name="Email" component="div" className="has-error" />
                  </div>
                  <div className="form-group m-3">
                    <div className="password-wrapper">
                      <FloatingLabel controlId="floatingInputGrid" label="Password">
                        <Field
                          name="Password"
                          className="form-control"
                          placeholder=""
                          type={showPassword ? "text" : "password"}
                        />
                      </FloatingLabel>
                      <span className="password-icon" onClick={togglePasswordVisibility}>
                        {showPassword ? <Slasheyeicon /> : <Eyeicon />}
                      </span>
                    </div>
                    <ErrorMessage name="Password" component="div" className="has-error" />
                  </div>
                  <Col lg={6} md={6} className="mb-0 d-grid gap-2 cent-2">
                    <Button
                      className="orange-rounded-btn"
                      variant="primary"
                      type="submit">
                      Sign In
                    </Button>
                  </Col>
                </Form>
                <div className="cent mt-2">
                  <span>
                    Forgot Password? Click
                    <Link to="/authentication/signup" className="ms-1 ">
                      here
                    </Link>
                    &nbsp;to reset.
                  </span>
                </div>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      </Formik>
    </Fragment>
  );
};

export default SignIn;
